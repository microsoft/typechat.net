// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public interface IConstraintValidatable
{
    void ValidateConstraints(ConstraintCheckContext context);
}
