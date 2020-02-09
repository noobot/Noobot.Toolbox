﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
                    ValidHandles = StartsWithHandle.For("schedule hourly"),
                    Description = "Schedule a command to execute every hour on the current channel. Usage: `@{bot} schedule hourly @{bot} tell me a joke`",
                    EvaluatorFunc = HourlyHandler,
                },
                new HandlerMapping
                {
                    ValidHandles = StartsWithHandle.For("schedule daily"),
                    Description = "Schedule a command to execute every day on the current channel. Usage: `@{bot} schedule daily @{bot} tell me a joke`",
                    EvaluatorFunc = DayHandler,
                },
                new HandlerMapping
                {
                    ValidHandles = StartsWithHandle.For("schedule cronjob"),
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
                    ValidHandles = StartsWithHandle.For("schedule delete"),
                    Description = "Delete a schedule in this channel. You must enter a valid {guid}",
                    EvaluatorFunc = DeleteHandlerForChannel,
                },
            };
        }

        private async IAsyncEnumerable<ResponseMessage> HourlyHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            var minutesPastTheHour = DateTime.Now.Minute;
            var schedule = $"0 {minutesPastTheHour} */1 * * ?";
            var command = message.TargetedText.Substring(matchedHandle.HandleHelpText.Length).Trim();

            yield return await CreateSchedule(message, command, schedule);
        }

        private async IAsyncEnumerable<ResponseMessage> DayHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            var minutesPastTheHour = DateTime.Now.Minute - 1;
            var hourOfDay = DateTime.Now.Hour;
            var schedule = $"0 {minutesPastTheHour} {hourOfDay} * * ?";
            var command = message.TargetedText.Substring(matchedHandle.HandleHelpText.Length).Trim();

            yield return await CreateSchedule(message, command, schedule);
        }

        private async IAsyncEnumerable<ResponseMessage> CronHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            var cronJob = message.TargetedText.Substring(matchedHandle.HandleHelpText.Length).Trim();
            var regexMatch = CronFormat.Match(cronJob);

            if (!regexMatch.Success)
            {
                yield return message.ReplyToChannel($"Error while parsing cron job. Your command should match something like `@{message.BotName} schedule cronjob '0 15 10 * * ?' @{message.BotName} tell me a joke``");
                yield break;
            }

            var schedule = regexMatch.Groups[1].Value.Trim();
            var command = regexMatch.Groups[2].Value.Trim();

            yield return await CreateSchedule(message, command, schedule);
        }

        private async Task<ResponseMessage> CreateSchedule(IncomingMessage message, string command, string cronSchedule)
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
                return await Task.FromResult(message.ReplyToChannel($"Unknown cron schedule `'{cronSchedule}'`"));
            }

            if (string.IsNullOrEmpty(schedule.Command))
            {
                return message.ReplyToChannel("Please enter a command to be scheduled.");
            }

            _schedulePlugin.AddSchedule(schedule);
            return message.ReplyToChannel($"Schedule created for command '{schedule.Command}'.");
        }

        private async IAsyncEnumerable<ResponseMessage> ListHandlerForChannel(IncomingMessage message, IValidHandle matchedHandle)
        {
            var schedules = _schedulePlugin.ListSchedulesForChannel(message.Channel);

            if (schedules.Any())
            {
                yield return await Task.FromResult(message.ReplyToChannel("Schedules for channel:"));

                string[] scheduleStrings = schedules.Select(x => x.ToString()).ToArray();
                yield return message.ReplyToChannel(">>>" + string.Join("\n", scheduleStrings));
            }
            else
            {
                yield return message.ReplyToChannel("No schedules set for this channel.");
            }
        }

        private async IAsyncEnumerable<ResponseMessage> DeleteHandlerForChannel(IncomingMessage message, IValidHandle matchedHandle)
        {
            var idString = message.TargetedText.Substring(matchedHandle.HandleHelpText.Length).Trim();

            if (Guid.TryParse(idString, out var guid))
            {
                var schedules = _schedulePlugin.ListSchedulesForChannel(message.Channel);
                var scheduleToDelete = schedules.FirstOrDefault(x => x.Guid == guid);

                if (scheduleToDelete == null)
                {
                    yield return await Task.FromResult(message.ReplyToChannel($"Unable to find schedule with GUID: `'{guid}'`"));
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
