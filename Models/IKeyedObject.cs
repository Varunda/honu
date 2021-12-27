
namespace watchtower.Models {

    /// <summary>
    ///     Represents an object that has an ID
    /// </summary>
    /// <remarks>
    ///     Not named IIDedObject, cause that looks weird with the double I
    /// </remarks>
    public interface IKeyedObject {

        /// <summary>
        ///     ID of the object
        /// </summary>
        int ID { get; set; }

    }

}