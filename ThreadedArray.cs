using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ImageMosaicGenerator
{
    public class SharedIncrementalArray
    {
        private readonly string[] _array;
        private int _index;

        public SharedIncrementalArray(string[] array)
        {
            _array = array;
            _index = -1;
        }

        public string GetNext(out int elementIndex)
        {
            lock (_array)
            {
                _index++;
                if (_index >= _array.Length)
                {
                    elementIndex = -1;
                    return null;
                }
                elementIndex = _index;
                return _array[_index];
            }
            
        }
        
    }
}