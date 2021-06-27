using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db {

    /// <summary>
    /// A patch performed on the database
    /// </summary>
    public interface IDbPatch {

        /// <summary>
        /// Minimum version, if the verison of the database is lower than this the patch won't be executed
        /// </summary>
        int MinVersion { get; }

        /// <summary>
        /// Name of the patch
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Execute the patch
        /// </summary>
        Task Execute(IDbHelper helper);

    }

    /// <summary>
    /// Attribute to attach 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PatchAttribute : Attribute { }

}
