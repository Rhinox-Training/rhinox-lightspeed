using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.IO;
using UnityEngine;
using UnityEngine.TestTools;

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
            AssertRelativePath("D:/Test/Foobar",   "D:/Test",     "Foobar\\");
            AssertRelativePath("D:/Test/Foobar/",  "D:/Test/",    "Foobar\\");
            AssertRelativePath("D:/Test/Foobar",   "D:/Test",     "Foobar\\");
            AssertRelativePath("D:/Test/Foobar\\", "D:/Test/",    "Foobar\\");
            AssertRelativePath("D:/Test\\Foobar",  "D:\\Test",    "Foobar\\");
            AssertRelativePath("D:/Test/Foobar",   "D:\\Test/",   "Foobar\\");
            AssertRelativePath("D:\\Test/Foobar",  "D:\\Test/",   "Foobar\\");
            AssertRelativePath("D:\\Test/Foobar",  "D:\\Test\\",  "Foobar\\");
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
