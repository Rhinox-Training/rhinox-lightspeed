using System.Collections;
using NUnit.Framework;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.IO;

namespace Tests
{
    public class FileHelperTests
    {
        [Test]
        public void EqualFolderTest()
        {
            AssertRelativePath("D:/Test", "D:/Test", string.Empty);
            AssertRelativePath("D:/Test", "D:/Test/", string.Empty);
            AssertRelativePath("D:/Test/", "D:/Test", string.Empty);
            AssertRelativePath("D:/Test/", "D:/Test/", string.Empty);
            AssertRelativePath("D:/Test", "D:\\Test", string.Empty);
            AssertRelativePath("D:/Test", "D:\\Test/", string.Empty);
            AssertRelativePath("D:\\Test", "D:\\Test/", string.Empty);
            AssertRelativePath("D:\\Test", "D:\\Test\\", string.Empty);
        }
        
        [Test]
        public void FindSubFolderTest()
        {
            AssertRelativePath("D:/Test/Foobar",   "D:/Test",     "Foobar");
            AssertRelativePath("D:/Test/Foobar",   "D:/Test",     "Foobar");
            AssertRelativePath("D:/Test\\Foobar",  "D:\\Test",    "Foobar");
            AssertRelativePath("D:/Test/Foobar",   "D:\\Test/",   "Foobar");
            AssertRelativePath("D:\\Test/Foobar",  "D:\\Test/",   "Foobar");
            AssertRelativePath("D:\\Test/Foobar",  "D:\\Test\\",  "Foobar");
            
            AssertRelativePath("D:/Test/Foobar/",  "D:/Test/",    "Foobar\\");
            AssertRelativePath("D:/Test/Foobar\\", "D:/Test/",    "Foobar\\");
        }
        
        [Test]
        public void FindSubFolderUpDownTest1()
        {
            AssertRelativePath("D:/Test/Bar/Foobar",   "D:/Test/Foo/",     "..\\Bar\\Foobar");
            AssertRelativePath("D:/Test/Bar/Foobar",   "D:/Test/Foo\\",    "..\\Bar\\Foobar");
            AssertRelativePath("D:/Test/Bar/Foobar",   "D:\\Test/Foo/",    "..\\Bar\\Foobar");
            AssertRelativePath("D:\\Test/Bar/Foobar",  "D:\\Test\\Foo\\",  "..\\Bar\\Foobar");
            AssertRelativePath("D:/Test\\Bar/Foobar",  "D:\\Test/Foo",     "..\\Bar\\Foobar");
            AssertRelativePath("D:\\Test/Bar/Foobar",  "D:\\Test/Foo",     "..\\Bar\\Foobar");
            
            AssertRelativePath("D:/Test/Bar/Foobar/",  "D:/Test/Foo",      "..\\Bar\\Foobar\\");
            AssertRelativePath("D:/Test/Bar/Foobar\\", "D:/Test/Foo",      "..\\Bar\\Foobar\\");
        }
        
        [Test]
        public void FindSubFolderUpDownTest2()
        {
            AssertRelativePath("D:/Test/Foo",      "D:/Test/Bar/Foobar/",  "..\\..\\Foo");
            AssertRelativePath("D:/Test/Foo",      "D:/Test/Bar/Foobar\\", "..\\..\\Foo");
            AssertRelativePath("D:\\Test/Foo",     "D:/Test\\Bar/Foobar",  "..\\..\\Foo");
            AssertRelativePath("D:\\Test/Foo",     "D:\\Test/Bar/Foobar",  "..\\..\\Foo");
            
            AssertRelativePath("D:/Test/Foo/",     "D:/Test/Bar/Foobar",   "..\\..\\Foo\\");
            AssertRelativePath("D:\\Test/Foo/",    "D:/Test/Bar/Foobar",   "..\\..\\Foo\\");
            AssertRelativePath("D:/Test/Foo\\",    "D:/Test/Bar/Foobar",   "..\\..\\Foo\\");
            AssertRelativePath("D:\\Test\\Foo\\",  "D:\\Test/Bar/Foobar",  "..\\..\\Foo\\");
        }

        [Test]
        public void FolderToFileTests()
        {
            AssertRelativePath("D:/Test/foobar.txt",   "D:/Test",     "foobar.txt");
            AssertRelativePath("D:/Test/foobar.txt",   "D:/Test/",    "foobar.txt");
            AssertRelativePath("D:/Test/foobar.txt",   "D:/Test",     "foobar.txt");
            AssertRelativePath("D:/Test/foobar.txt",   "D:/Test/",    "foobar.txt");
            AssertRelativePath("D:/Test\\foobar.txt",  "D:\\Test",    "foobar.txt");
            AssertRelativePath("D:/Test/foobar.txt",   "D:\\Test/",   "foobar.txt");
            AssertRelativePath("D:\\Test/foobar.txt",  "D:\\Test/",   "foobar.txt");
            AssertRelativePath("D:\\Test/foobar.txt",  "D:\\Test\\",  "foobar.txt");
        }

        public void AssertRelativePath(string fromPath, string parentPath, string expectedAnswer)
        {
            string relativePath = FileHelper.GetRelativePath(fromPath, parentPath);
            Assert.True((relativePath.IsNullOrEmpty() && expectedAnswer.IsNullOrEmpty()) || relativePath == expectedAnswer, 
                $"'{parentPath}' => '{fromPath}' yielded '{relativePath}', expected '{expectedAnswer}'");
        }
    }
}
