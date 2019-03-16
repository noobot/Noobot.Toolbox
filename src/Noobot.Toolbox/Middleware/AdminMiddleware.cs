using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Noobot.Core;
using Noobot.Core.MessagingPipeline.Middleware;
using Noobot.Core.MessagingPipeline.Middleware.ValidHandles;
using Noobot.Core.MessagingPipeline.Request;
using Noobot.Core.MessagingPipeline.Response;
using Noobot.Toolbox.Plugins;

namespace Noobot.Toolbox.Middleware
{
    /// <summary>
    /// Requires config entry 'adminPin' to be populated. Given a user authenticated, this plugin can give users extra abilities/functions.
    /// A good example of how Middleware and Plugins can work together.
    /// </summary>
    public class AdminMiddleware : MiddlewareBase
    {
        private readonly AdminPlugin _adminPlugin;
        private readonly SchedulePlugin _schedulePlugin;
        private readonly INoobotCore _noobotCore;
        private readonly ILogger _log;

        public AdminMiddleware(
            IMiddleware next, 
            AdminPlugin adminPlugin, 
            SchedulePlugin schedulePlugin, 
            INoobotCore noobotCore, 
            ILogger log) : base(next)
        {
            _adminPlugin = adminPlugin;
            _schedulePlugin = schedulePlugin;
            _noobotCore = noobotCore;
            _log = log;

            HandlerMappings = new[]
            {
                new HandlerMapping
                {
                    ValidHandles = ExactMatchHandle.For("admin pin"),
                    EvaluatorFunc = PinHandler,
                    Description = "This function is used to authenticate a user as admin",
                    VisibleInHelp = false
                },
                new HandlerMapping
                {
                    ValidHandles = ExactMatchHandle.For("admin schedules list"),
                    EvaluatorFunc = SchedulesListHandler,
                    Description = "[Requires authentication] Will return a list of all schedules.",
                    VisibleInHelp = false
                },
                new HandlerMapping
                {
                    ValidHandles = ExactMatchHandle.For("admin schedules delete"),
                    EvaluatorFunc = DeleteSchedulesHandler,
                    Description = "[Requires authentication] This will delete all schedules.",
                    VisibleInHelp = false
                },
                new HandlerMapping
                {
                    ValidHandles = ExactMatchHandle.For("admin channels"),
                    EvaluatorFunc = ChannelsHandler,
                    Description = "[Requires authentication] Will return all channels connected.",
                    VisibleInHelp = false
                },
                new HandlerMapping
                {
                    ValidHandles = ExactMatchHandle.For("admin help", "admin list"),
                    EvaluatorFunc = AdminHelpHandler,
                    Description = "[Requires authentication] Lists all available admin functions",
                    VisibleInHelp = false
                }
            };
        }

        private IEnumerable<ResponseMessage> PinHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            if (!_adminPlugin.AdminModeEnabled())
            {
                yield return message.ReplyToChannel("Admin mode isn't enabled.");
                yield break;
            }

            string pinString = message.TargetedText.Substring(matchedHandle.HandleHelpText.Length).Trim();

            if (int.TryParse(pinString, out var pin))
            {
                if (_adminPlugin.AuthoriseUser(message.UserId, pin))
                {
                    yield return message.ReplyToChannel($"{message.Username} - you now have admin rights.");
                    _log.LogInformation($"{message.Username} now has admin rights.");
                }
                else
                {
                    yield return message.ReplyToChannel("Incorrect admin pin entered.");
                }
            }
            else
            {
                yield return message.ReplyToChannel($"Unable to parse pin '{pinString}'");
            }
        }

        private IEnumerable<ResponseMessage> AdminHelpHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            if (!_adminPlugin.AuthenticateUser(message.UserId))
            {
                yield return message.ReplyToChannel($"Sorry {message.Username}, only admins can use this function.");
                yield break;
            }

            foreach (var handlerMapping in HandlerMappings)
            {
                string mappings = string.Join(" | ", handlerMapping.ValidHandles.Select(x => $"{x}"));
                yield return message.ReplyDirectlyToUser($"`{mappings}`    - {handlerMapping.Description}");
            }
        }

        private IEnumerable<ResponseMessage> SchedulesListHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            if (!_adminPlugin.AuthenticateUser(message.UserId))
            {
                yield return message.ReplyToChannel($"Sorry {message.Username}, only admins can use this function.");
                yield break;
            }

            var schedules = _schedulePlugin.ListAllSchedules();
            string[] scheduleStrings = schedules.Select(x => $"Guid: '{x.Guid}' Channel: '{x.Channel}'.").ToArray();

            yield return message.ReplyToChannel("All Schedules:");
            yield return message.ReplyToChannel(">>>" + string.Join("\n", scheduleStrings));
        }

        private IEnumerable<ResponseMessage> DeleteSchedulesHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            if (!_adminPlugin.AuthenticateUser(message.UserId))
            {
                yield return message.ReplyToChannel($"Sorry {message.Username}, only admins can use this function.");
                yield break;
            }

            var schedules = _schedulePlugin.ListAllSchedules();
            _schedulePlugin.DeleteSchedules(schedules.Select(x => x.Guid).ToArray());

            yield return message.ReplyToChannel("All schedules deleted");
        }

        private IEnumerable<ResponseMessage> ChannelsHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            if (!_adminPlugin.AuthenticateUser(message.UserId))
            {
                yield return message.ReplyToChannel($"Sorry {message.Username}, only admins can use this function.");
                yield break;
            }

            Dictionary<string, string> channels = _noobotCore.ListChannels();
            yield return message.ReplyToChannel("All Connected Channels:");
            yield return message.ReplyToChannel(">>>" + string.Join("\n", channels.Select(x => $"{x.Key}: {x.Value}")));
        }
    }
}