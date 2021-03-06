﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfreeze.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfreeze.Controllers
{
    [Route("api/ac")]
    [ApiController]
    public class FreezersController : ControllerBase
    {
        private readonly IFreezerTasksProcessor _processor;
        private readonly IFreezerStateHolder _stateHolder;
        private readonly IAnalyticsService _analytics;
        private readonly ILogger _logger;

        public FreezersController(IFreezerStateHolder stateHolder,
            IFreezerTasksProcessor processor,
            IAnalyticsService analytics,
            ILogger<FreezersController> logger)
        {
            _stateHolder = stateHolder;
            _processor = processor;
            _analytics = analytics;
            _logger = logger;
        }

        // GET api/ac/list
        [HttpGet("list")]
        public JsonResult List()
        {
            _analytics.SendEvent("List");
            var result = _stateHolder.GetFreezers();
            return new JsonResult(result);
        }

        // POST api/ac/enable/8/42
        [HttpPost("enable/{floor}/{id}")]
        public JsonResult Enable(int floor, int id)
        {
            TrackToggleAnalytics("on", floor, id);
            var task = new FreezerTask(floor, id, true);
            _processor.Enqueue(task);
            var result = _stateHolder.GetFreezers();
            return new JsonResult(result);
        }

        // POST api/ac/disable/5/24
        [HttpPost("disable/{floor}/{id}")]
        public JsonResult Disable(int floor, int id)
        {
            TrackToggleAnalytics("off", floor, id);
            var task = new FreezerTask(floor, id, false);
            _processor.Enqueue(task);
            var result = _stateHolder.GetFreezers();
            return new JsonResult(result);
        }

        private void TrackToggleAnalytics(string toggle, int floor, int id)
        {
            FreezerModel freezer;
            string place = null;
            string name = null;

            if (DefaultState.Freezers.TryGetValue(new FreezerIdentifier(floor, id), out freezer))
            {
                place = freezer.Place;
                name = freezer.Name;
            }

            _analytics.SendEvent("Toggle", new Dictionary<string, object>
            {
                ["toggle"] = toggle,
                ["floor"] = floor,
                ["id"] = id,
                ["place"] = place ?? "unknown",
                ["name"] = name ?? "unknown",
            });
        }
    }
}
