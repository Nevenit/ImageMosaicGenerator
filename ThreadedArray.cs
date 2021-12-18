using System;
using System.Collections.Generic;
using System.Threading;

namespace ImageMosaicGenerator
{
    public class SharedArray
    {
        private readonly string[] _array;
        private int _index;
        private bool _isBeingRead;
        private readonly List<int> _threadQueue;

        public SharedArray(string[] array)
        {
            _array = array;
            _index = -1;
            _isBeingRead = false;
            _threadQueue = new List<int>();
        }

        public string GetNext(int threadId, out int elementIndex)
        {
            // Check if the array is currently being read
            if (_isBeingRead)
            {
                // Join queue
                JoinQueue(threadId);
                
                // Wait until array is ready to be read
                while (_isBeingRead && _threadQueue[0] != threadId)
                {
                    //wait...
                }
            }

            // Stop other threads from accessing the array
            _isBeingRead = true;
            
            // Increment the array position
            _index++;
            
            // Store the wanted element in a temporary variable
            string elementToReturn = _array[_index];
            
            // Remove this thread from the queue
            LeaveQueue(threadId);
            
            // Allow the next thread to read the array
            _isBeingRead = false;
            
            // Return the element and the index
            elementIndex = _index;
            return elementToReturn;
        }

        // Remove a thread from the queue
        private void LeaveQueue(int threadId)
        {
            _threadQueue.Remove(threadId);
        }
        
        // Adds thread to queue
        private void JoinQueue(int threadId)
        {
            // The thread shouldn't be in the queue more than once
            // In case it does somehow happen exit with code 101
            if (_threadQueue.Contains(threadId))
                Environment.Exit(101);
            
            // Add thread to the end of queue
            _threadQueue.Add(threadId);
        }
        
    }
}