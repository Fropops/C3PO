using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent
{

    public static class Extension
    {
        public static string[] GetArgs(this string src)
        {
            var res = new List<string>();
            src = src.Trim();

            bool inQuotes = false;
            bool inDoubleQuotes = false;

            string currentValue = string.Empty;
            foreach (var c in src)
            {
                if (c == '\"')
                {
                    if (!inQuotes)
                    {
                        if (inDoubleQuotes)
                        {
                            //end of params => 
                            inDoubleQuotes = false;
                            res.Add(currentValue);
                            currentValue = string.Empty;
                        }
                        else
                            inDoubleQuotes = true;
                    }
                    else
                    {
                        currentValue += c;
                    }
                    continue;
                }

                if (c== '\'')
                {
                    if (!inDoubleQuotes)
                    {
                        if (inQuotes)
                        {
                            //end of params => 
                            inQuotes = false;
                            res.Add(currentValue);
                            currentValue = string.Empty;
                        }
                        else
                            inQuotes = true;
                    }
                    else
                    {
                        currentValue += c;
                    }
                    continue;
                }

                if (c == ' ')
                {
                    if (!inQuotes && !inDoubleQuotes)
                    {
                        if (!string.IsNullOrEmpty(currentValue))
                        {
                            res.Add(currentValue);
                            currentValue = string.Empty;
                        }
                    }
                    else
                        currentValue += c;
                    continue;
                }

                currentValue += c;
            }


            if (!string.IsNullOrEmpty(currentValue))
            {
                res.Add(currentValue);
                currentValue = string.Empty;
            }

            return res.ToArray();
        }
    }

}
