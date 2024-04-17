public interface ICustomFileSystem
{
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    void WriteAllText(string path, string contents);
}
