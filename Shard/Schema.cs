using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Shard.Storage;

namespace Shard
{
    /// <summary>
    /// Object to represent a Git repository
    /// </summary>
    public class Schema
    {
        private ObjectStorage _objectStorage;

        #region Constructors
        internal Schema(string workingDirectory)
        {
            if (workingDirectory == null)
                throw new ArgumentNullException("workingDirectory", "You must specify a workingDirectory or gitDirectory");

            string gitDirectory = Path.Combine(workingDirectory, ".git");


            if (!Directory.Exists(gitDirectory))
                Directory.CreateDirectory(gitDirectory);

            this.Location = Helper.MakeAbsolutePath(gitDirectory);

            _objectStorage = new ObjectStorage(this.Location);
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// Gets the location of the repo.
        /// </summary>
        public string Location { get; private set; }


        #endregion

        #region Public Methods

        public void Put<T>(T data)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string id)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
