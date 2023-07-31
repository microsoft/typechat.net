// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.TypeChat.Schema;

namespace Math;

public interface API
{
    [Comment("Add two numbers")]
    double add(double x, double y);
    [Comment("Subtract two numbers")]
    double sub(double x, double y);
    [Comment("Multiply two numbers")]
    double mul(double x, double y);
    [Comment("Divide two numbers")]
    double div(double x, double y);
    [Comment("Negate a number")]
    double neg(double x);
    [Comment("Identity function")]
    double id(double x);
    [Comment("Unknown request")]
    double unknown(string text);
}
