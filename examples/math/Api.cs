// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.TypeChat.Schema;

namespace Math;

public interface API
{
    [Comment("Add two numbers")]
    double Add(double x, double y);
    [Comment("Subtract two numbers")]
    double Sub(double x, double y);
    [Comment("Multiply two numbers")]
    double Mul(double x, double y);
    [Comment("Divide two numbers")]
    double Div(double x, double y);
    [Comment("Negate a number")]
    double Neg(double x);
    [Comment("Identity function")]
    double id(double x);
    [Comment("Unknown request")]
    double unknown(string text);
}
