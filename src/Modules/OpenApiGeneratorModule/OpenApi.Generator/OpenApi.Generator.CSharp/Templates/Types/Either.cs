using System;

namespace Templates.Types
{
    public abstract class Either<TLeft, TRight>
    {
        public abstract void IfLeft(Action<TLeft> action);
        public abstract void IfRight(Action<TRight> action);
        public abstract Either<TLeft, T1Right> Select<T1Right>(Func<TRight, T1Right> mapping);
        public abstract TResult Match<TResult>(Func<TLeft, TResult> Left, Func<TRight, TResult> Right);
    }

    public class Left<TLeft, TRight> : Either<TLeft, TRight>
    {
        private readonly TLeft Value;

        public Left(TLeft left)
        {
            Value = left;
        }
        public override void IfLeft(Action<TLeft> action)
        {
            action(Value);
        }

        public override void IfRight(Action<TRight> action) { }

        public override TResult Match<TResult>(Func<TLeft, TResult> Left, Func<TRight, TResult> Right)
        {
            return Left(Value);
        }

        public override Either<TLeft, T1Right> Select<T1Right>(Func<TRight, T1Right> mapping)
        {
            return new Left<TLeft, T1Right>(Value);
        }
    }

    public class Right<TLeft, TRight> : Either<TLeft, TRight>
    {
        private readonly TRight Value;

        public Right(TRight value)
        {
            Value = value;
        }
        public override void IfLeft(Action<TLeft> action) { }

        public override void IfRight(Action<TRight> action)
        {
            action(Value);
        }

        public override Either<TLeft, T1Right> Select<T1Right>(Func<TRight, T1Right> mapping)
        {
            return new Right<TLeft, T1Right>(mapping(Value));
        }
        public override TResult Match<TResult>(Func<TLeft, TResult> Left, Func<TRight, TResult> Right)
        {
            return Right(Value);
        }
    }
}
