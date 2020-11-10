using CommonUtil;
using Identity.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace Identity.API.Controllers
{
    [ApiController]
    public class Controller : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly UnitOfWork _unitOfWork;
        public Controller(UnitOfWork unitOfWork, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        protected IActionResult Error(string errorMessage)
        {
            _logger.LogWarning("Validation Failed: {0}", errorMessage);
            return BadRequest(Envelope.Error<object>(errorMessage));
        }

        protected IActionResult Error<T>(string errorMessage)
        {
            _logger.LogWarning("Validation Failed: {0}", errorMessage);
            return BadRequest(Envelope.Error<T>(errorMessage));
        }

        protected IActionResult Failure(string errorMessage)
        {
            _logger.LogError(errorMessage);
            return StatusCode(500, Envelope.Error<object>(errorMessage));
        }

        protected IActionResult Failure<T>(string errorMessage, T ex) where T : Exception
        {
            _logger.LogError(ex, errorMessage);
            return StatusCode(500, Envelope.Error<object>(errorMessage));
        }

        protected IActionResult Ok(bool contextReadonly = false)
        {
            bool success = contextReadonly || _unitOfWork.Commit(_logger);
            if (!success)
                return Failure("An error occured while saving changes");

            return base.Ok(Envelope.Ok());
        }

        protected IActionResult Ok<T>(T result, bool contextReadonly = false)
        {
            bool success = contextReadonly || _unitOfWork.Commit(_logger);
            if (!success)
                return Failure("An error occured while saving changes");

            return base.Ok(Envelope.Ok(result));
        }

        protected IActionResult Forbidden()
        {
            _logger.LogWarning("Permission Denied: No sufficient permission to process requested resources");
            return base.Forbid();
        }
    }
}
