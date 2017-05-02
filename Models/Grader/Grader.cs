using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BobTheGrader.Models
{
    /// <summary>
    /// This is an abstract class that initializes the appropriate grader.
    /// </summary>
    abstract class Grader
    {
        /// <summary>
        /// Creates the grader according to the question type.
        /// </summary>
        /// <param name="type">Question type.</param>
        /// <returns>An instance of the grader.</returns>
        public static Grader createGrader(string type)
        {
            Grader grader = null;
            if (type.Equals("Exact Matching")) grader = new EMGrader();
            else if (type.Equals("Regular Expression")) grader = new REGrader();
            return grader;
        }
        /// <summary>
        /// Corrects the specified file.
        /// </summary>
        /// <param name="file">The file to be graded.</param>
        /// <param name="output">Expected output.</param>
        /// <returns>Line of error.</returns>
        public abstract int correct(string file, string output);
    }
}
