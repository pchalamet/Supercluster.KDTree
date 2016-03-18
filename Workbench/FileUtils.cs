namespace Workbench
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    public static class FileUtils
    {

        public static void ToFile(this object obj, string path)
        {
            // To serialize the hashtable and its key/value pairs,  
            // you must first open a stream for writing. 
            // In this case, use a file stream.
            using (var fs = new FileStream(path, FileMode.Create))
            {
                // Construct a BinaryFormatter and use it to serialize the data to the stream.
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, obj);

            }
        }


        public static T FromFile<T>(string path)
        {
            T obj;

            // Open the file containing the data that you want to deserialize.
            using (var fs = new FileStream(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                obj = (T)formatter.Deserialize(fs);
            }

            return obj;
        }
    }
}