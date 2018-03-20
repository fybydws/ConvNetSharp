﻿using System;
using ConvNetSharp.Volume;

namespace ConvNetSharp.Flow.Ops
{
    /// <summary>
    ///     Assignment: valueOp = op
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Assign<T> : Op<T> where T : struct, IEquatable<T>, IFormattable
    {
        private long _lastComputeStep;

        public Assign(Op<T> valueOp, Op<T> op)
        {
            if (!(valueOp is Variable<T>))
            {
                throw new ArgumentException("Assigned Op should be a Variable", nameof(valueOp));
            }

            AddParent(valueOp);
            AddParent(op);
        }

        public override string Representation => "->";

        public override void Differentiate()
        {
            throw new NotImplementedException();
        }

        public override Volume<T> Evaluate(Session<T> session)
        {
            if (this._lastComputeStep == session.Step)
            {
                return this.Parents[0].Evaluate(session);
            }
            this._lastComputeStep = session.Step;

            this.Result = this.Parents[1].Evaluate(session);
            ((Variable<T>)this.Parents[0]).SetValue(this.Result);

            return base.Evaluate(session);
        }
    }
}