namespace Utilities
{
    //--------------------------------------------------Klasa powiększająca tablice------------------------------------------------------------------
    public static class ExpandArr
    {
        public static T[] Expand<T>(T[] arr)
        {
            var temp = arr;
            arr = new T[temp.Length + 1];
            for (var i = 0; i < temp.Length; i++)
            {
                arr[i] = temp[i];
            }
            return arr;
        }
    }
}
