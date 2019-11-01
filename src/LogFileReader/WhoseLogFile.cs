namespace LogFileReader
{
    public class WhoseLogFile
    {
        public string GetCharacterNameFromLogFile(string filePath)
        {
            // Log file has standard name format: 'eqlog_<charName>_<serverName>.txt'
            var firstUnder = filePath.IndexOf('_');
            if (firstUnder == -1)
                return null;

            var secondUnder = filePath.IndexOf('_', firstUnder + 1);
            if (secondUnder == -1)
                return null;

            return filePath.Substring(firstUnder + 1, secondUnder - firstUnder - 1);
        }
    }
}
