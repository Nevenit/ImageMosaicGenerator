using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImageMosaicGenerator
{
    /*class Argument
    {
        public enum Types
        {
            Int,
            String,
            Float
        }
        
        private string Name { get; set; }
        private string Option { get; set; }
        private Types Type { get; set; }
        
        private bool IsRequired { get; set; }
        private string HelpText { get; set; }

        public Argument(string name, string option, Types type, bool isRequired, string helpText)
        {
            Name = name;
            Option = option;
            Type = type;
            IsRequired = isRequired;
            HelpText = helpText;
        }
        
    }*/
    
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                throw new Exception("Missing arguments");
            }
            
            string imagePath = args[0];
            string tilesPath = args[1];
            
            var ext = new List<string> { ".jpg", ".png" };
            string[] tileImages = Directory.GetFiles(tilesPath, "*.*", SearchOption.AllDirectories).Where(s => ext.Contains(Path.GetExtension(s))).ToArray();
            
        }

        
    }
}