using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BobTheGrader.Models
{
    class EMGrader : Grader
    {
        public override int correct(string file, string output)
        {
            StreamReader read = new StreamReader(file);
            StringReader oread = new StringReader(output);
            string line = read.ReadLine();
            string oline = oread.ReadLine();
            if(line != null && oline != null)
            {
                int i = 1;
                while (line != null && oline != null)
                {
                    if (!line.Equals(oline)) return i;
                    i++;
                    line = read.ReadLine();
                    oline = oread.ReadLine();
                }

                //excess in sub
                if (line != null)
                {
                    return -i;
                }

                //missing in sub
                if (oline != null)
                {
                    return i;
                }

                return 0;
            }
            //empty sub
            return -10001;
        }
    }
}
