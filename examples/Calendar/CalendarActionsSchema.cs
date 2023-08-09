// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;

namespace Calendar;

[Comment("The following types define the structure of an object of type CalendarActions that represents a list of requested calendar actions")]
public class CalendarActions
{
    [JsonPropertyName("actions")]
    public Action[] Actions { get; set; }
}

[JsonPolymorphic]
[JsonDerivedType(typeof(AddEventAction), typeDiscriminator: nameof(AddEventAction))]
[JsonDerivedType(typeof(RemoveEventAction), typeDiscriminator: nameof(RemoveEventAction))]
[JsonDerivedType(typeof(AddParticipantsAction), typeDiscriminator: nameof(AddParticipantsAction))]
[JsonDerivedType(typeof(ChangeTimeRangeAction), typeDiscriminator: nameof(ChangeTimeRangeAction))]
[JsonDerivedType(typeof(ChangeDescriptionAction), typeDiscriminator: nameof(ChangeDescriptionAction))]
[JsonDerivedType(typeof(FindEventsAction), typeDiscriminator: nameof(FindEventsAction))]
[JsonDerivedType(typeof(UnknownAction), typeDiscriminator: nameof(UnknownAction))]
public abstract partial class Action { }

public class AddEventAction : Action
{
    [JsonPropertyName("event")]
    public Event Event { get; set; }
}

public class RemoveEventAction : Action
{
    [JsonPropertyName("eventReference")]
    public EventReference EventReference { get; set; }
}

public class AddParticipantsAction : Action
{
    [Comment("event to be augmented; if not specified assume last event discussed")]
    [JsonPropertyName("eventReference")]
    public EventReference? EventReference { get; set; }

    [Comment("new participants (one or more)")]
    [JsonPropertyName("participants")]
    public string[] Participants { get; set; }
}

public class ChangeTimeRangeAction : Action
{
    [Comment("event to be changed")]
    [JsonPropertyName("eventReference")]
    public EventReference? EventReference { get; set; }

    [Comment("new time range for the event")]
    [JsonPropertyName("timeRange")]
    public EventTimeRange TimeRange { get; set; }
}

public class ChangeDescriptionAction : Action
{
    [Comment("event to be changed")]
    [JsonPropertyName("eventReference")]
    public EventReference? EventReference { get; set; }

    [Comment("new description for the event")]
    [JsonPropertyName("description")]
    public string Description { get; set; }
}


public class FindEventsAction : Action
{
    // one or more event properties to use to search for matching events
    [JsonPropertyName("eventReference")]
    public EventReference EventReference { get; set; }
}

[Comment("if the user types text that can not easily be understood as a calendar action, this action is used")]
public partial class UnknownAction : Action
{
    [Comment("The text that wasn't understood")]
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public class EventTimeRange
{
    [JsonPropertyName("startTime")]
    public string? StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public string? EndTime { get; set; }

    [JsonPropertyName("duration")]
    public string? Duration{ get; set; }
}

public class Event
{
    [Comment("date (example: March 22, 2024) or relative date (example: after EventReference)")]
    [JsonPropertyName("day")]
    public string Day { get; set; }

    [JsonPropertyName("timeRange")]
    public EventTimeRange TimeRange { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [Comment("a list of people or named groups like 'team'")]
    [JsonPropertyName("participants")]
    public string[]? Participants { get; set; }
}

[Comment("properties used by the requester in referring to an event")]
[Comment("these properties are only specified if given directly by the requester")]
public class EventReference
{
    [Comment("date (example: March 22, 2024) or relative date (example: after EventReference)")]
    [JsonPropertyName("day")]
    public string? Day { get; set; }

    [Comment("(examples: this month, this week, in the next two days)")]
    [JsonPropertyName("dayRange")]
    public string? DayRange { get; set; }

    [JsonPropertyName("timeRange")]
    public EventTimeRange? TimeRange { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("participants")]
    public string[]? Participants { get; set; }
}
