
namespace Drones.Interface
{
    using Drones.Utils;
    using Drones.UI;

    public delegate void ListChangeHandler();

    public interface IListWindow
    {
        ListTupleContainer TupleContainer { get; }

        ListElement TupleType { get; }

        event ListChangeHandler ListChanged;
    }

}