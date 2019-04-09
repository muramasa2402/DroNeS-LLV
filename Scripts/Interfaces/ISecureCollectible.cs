using System;

namespace Drones.Interface
{
    public delegate void AlertHandler<T>(T item);

    public interface ISecureCollectible<T>
    {
        event AlertHandler<T> SetChanged;

        event AlertHandler<T> ItemAdded;

        event AlertHandler<T> ItemRemoved;

        Predicate<T> MemberCondition { get; set; }
    }
}
