// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public interface IConstraintValidatable
{
    void ValidateConstraints(ConstraintCheckContext context);
}

public static class ConstraintValidatorEx
{
    public static void ValidateConstraints(this object obj, ConstraintCheckContext context)
    {
        if (obj != null &&
            obj is IConstraintValidatable validator)
        {
            validator.ValidateConstraints(context);
        }
    }

    public static void ValidateConstraints(this IEnumerable<object> objects, ConstraintCheckContext context)
    {
        if (objects != null)
        {
            foreach (var obj in objects)
            {
                if (obj is IConstraintValidatable validator)
                {
                    validator.ValidateConstraints(context);
                }
            }
        }
    }
}
