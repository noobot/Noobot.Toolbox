using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Noobot.Core.MessagingPipeline.Middleware;
using Noobot.Core.MessagingPipeline.Middleware.ValidHandles;
using Noobot.Core.MessagingPipeline.Request;
using Noobot.Core.MessagingPipeline.Response;
using Noobot.Toolbox.Plugins;
using Noobot.Toolbox.Plugins.Scheduling;
using Quartz;

namespace Noobot.Toolbox.Middleware
{
    public class ScheduleMiddleware : MiddlewareBase
    {
        private readonly SchedulePlugin _schedulePlugin;
        private static readonly Regex CronFormat = new Regex(@"^\'(.*?)\'(.*?)$", RegexOptions.Compiled);

        public ScheduleMiddleware(IMiddleware next, SchedulePlugin schedulePlugin) : base(next)
        {
            _schedulePlugin = schedulePlugin;

            HandlerMappings = new[]
            {
                new HandlerMapping
                {
                    ValidHandles = ExactMatchHandle.For("schedule hourly"),
                    Description = "Schedule a command to execute every hour on the current channel. Usage: `@{bot} schedule hourly @{bot} tell me a joke`",
                    EvaluatorFunc = HourlyHandler,
                },
                new HandlerMapping
                {
                    ValidHandles = ExactMatchHandle.For("schedule daily"),
                    Description = "Schedule a command to execute every day on the current channel. Usage: `@{bot} schedule daily @{bot} tell me a joke`",
                    EvaluatorFunc = DayHandler,
                },
                new HandlerMapping
                {
                    ValidHandles = ExactMatchHandle.For("schedule cronjob"),
                    Description = "Schedule a cron job for this channel. Usage: `@{bot} schedule cronjob '0 15 10 * * ?' @{bot} tell me a joke`",
                    EvaluatorFunc = CronHandler,
                },
                new HandlerMapping
                {
                    ValidHandles = ExactMatchHandle.For("schedule list"),
                    Description = "List all schedules on the current channel",
                    EvaluatorFunc = ListHandlerForChannel,
                },
                new HandlerMapping
                {
                    ValidHandles = ExactMatchHandle.For("schedule delete"),
                    Description = "Delete a schedule in this channel. You must enter a valid {guid}",
                    EvaluatorFunc = DeleteHandlerForChannel,
                },
            };
        }

        private IEnumerable<ResponseMessage> HourlyHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            int minutesPastTheHour = DateTime.Now.Minute;
            string schedule = $"0 {minutesPastTheHour} */1 * * ?";
            string command = message.TargetedText.Substring(matchedHandle.HandleHelpText.Length).Trim();

            yield return CreateSchedule(message, command, schedule);
        }

        private IEnumerable<ResponseMessage> DayHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            int minutesPastTheHour = DateTime.Now.Minute - 1;
            int hourOfDay = DateTime.Now.Hour;
            string schedule = $"0 {minutesPastTheHour} {hourOfDay} * * ?";
            string command = message.TargetedText.Substring(matchedHandle.HandleHelpText.Length).Trim();

            yield return CreateSchedule(message, command, schedule);
        }

        private IEnumerable<ResponseMessage> CronHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            string cronJob = message.TargetedText.Substring(matchedHandle.HandleHelpText.Length).Trim();
            Match regexMatch = CronFormat.Match(cronJob);

            if (!regexMatch.Success)
            {
                yield return message.ReplyToChannel($"Error while parsing cron job. Your command should match something like `@{message.BotName} schedule cronjob '0 15 10 * * ?' @{message.BotName} tell me a joke``");
                yield break;
            }

            string schedule = regexMatch.Groups[1].Value.Trim();
            string command = regexMatch.Groups[2].Value.Trim();

            yield return CreateSchedule(message, command, schedule);
        }

        private ResponseMessage CreateSchedule(IncomingMessage message, string command, string cronSchedule)
        {
            var schedule = new ScheduleEntry
            {
                Guid = Guid.NewGuid(),
                Channel = message.Channel,
                ChannelType = message.ChannelType,
                Command = command,
                CronSchedule = cronSchedule,
                UserId = message.UserId,
                UserName = message.Username,
                Created = DateTime.Now
            };

            if (!CronExpression.IsValidExpression(cronSchedule))
            {
                return message.ReplyToChannel($"Unknown cron schedule `'{cronSchedule}'`");
            }

            if (string.IsNullOrEmpty(schedule.Command))
            {
                return message.ReplyToChannel("Please enter a command to be scheduled.");
            }

            _schedulePlugin.AddSchedule(schedule);
            return message.ReplyToChannel($"Schedule created for command '{schedule.Command}'.");
        }

        private IEnumerable<ResponseMessage> ListHandlerForChannel(IncomingMessage message, IValidHandle matchedHandle)
        {
            ScheduleEntry[] schedules = _schedulePlugin.ListSchedulesForChannel(message.Channel);

            if (schedules.Any())
            {
                yield return message.ReplyToChannel("Schedules for channel:");

                string[] scheduleStrings = schedules.Select(x => x.ToString()).ToArray();
                yield return message.ReplyToChannel(">>>" + string.Join("\n", scheduleStrings));
            }
            else
            {
                yield return message.ReplyToChannel("No schedules set for this channel.");
            }
        }

        private IEnumerable<ResponseMessage> DeleteHandlerForChannel(IncomingMessage message, IValidHandle matchedHandle)
        {
            string idString = message.TargetedText.Substring(matchedHandle.HandleHelpText.Length).Trim();
            Guid guid;

            if (Guid.TryParse(idString, out guid))
            {
                ScheduleEntry[] schedules = _schedulePlugin.ListSchedulesForChannel(message.Channel);
                ScheduleEntry scheduleToDelete = schedules.FirstOrDefault(x => x.Guid == guid);

                if (scheduleToDelete == null)
                {
                    yield return message.ReplyToChannel($"Unable to find schedule with GUID: `'{guid}'`");
                }
                else
                {
                    _schedulePlugin.DeleteSchedule(guid);
                    yield return message.ReplyToChannel($"Removed schedule: `{scheduleToDelete}`");
                }
            }
            else
            {
                yield return message.ReplyToChannel($"Invalid id entered. Try using `schedule list`. (`{idString}`)");
            }
        }
    }
}
