// MIT License
//
// Copyright (c) 2021 RealityStop
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
// USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace RealityStop.LinkMerge
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEditor.PackageManager;
    using UnityEditor.PackageManager.Requests;
    using UnityEngine;

    public class PackagesLinkXmlExtractor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public string TemporaryFolder => $"{Application.dataPath}/Temporary-Build/";

        public static string TemporaryFolderMeta => $"{Application.dataPath}/Temporary-Build.meta";

        public string LinkFilePath => $"{TemporaryFolder}link.xml";

        // We execute the script after the others ones
        public int callbackOrder => 10;

        public async void OnPreprocessBuild(BuildReport report)
        {
            // NOTE: We merge a potentially existing previous link.xml file instead of skipping preprocess step
            // if (!File.Exists(LinkFilePath))
            await CreateMergedLinkFromPackages();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (File.Exists(LinkFilePath))
                File.Delete(LinkFilePath);
            if (Directory.Exists(TemporaryFolder))
                Directory.Delete(TemporaryFolder, true);

            if (File.Exists(TemporaryFolderMeta))
                File.Delete(TemporaryFolderMeta);
        }

        private async Task CreateMergedLinkFromPackages()
        {
            ListRequest request = Client.List();

            do
            {
                await Task.Yield();
            } while (!request.IsCompleted);

            if (request.Status == StatusCode.Success)
            {
                List<string> xmlPathList = new List<string>();
                foreach (PackageInfo package in request.Result)
                {
                    string path = package.resolvedPath;
                    xmlPathList.AddRange(
                        Directory
                            .EnumerateFiles(path, "linkmerge.xml", SearchOption.AllDirectories)
                            .ToList()
                    );
                }

                if (xmlPathList.Count <= 0)
                    return;

                XDocument[] xmlList = xmlPathList.Select(XDocument.Load).ToArray();

                XDocument combinedXml = xmlList.First();
                foreach (XDocument xDocument in xmlList.Where(xml => xml != combinedXml))
                {
                    combinedXml.Root.Add(xDocument.Root.Elements());
                }

                if (!Directory.Exists(TemporaryFolder))
                {
                    Directory.CreateDirectory(TemporaryFolder);
                }
                else if (File.Exists(LinkFilePath))
                {
                    XDocument previousLinksXML = XDocument.Load(LinkFilePath);
                    combinedXml.Root.Add(previousLinksXML.Root.Elements());
                }

                combinedXml.Save(LinkFilePath);
            }
            else if (request.Status >= StatusCode.Failure)
            {
                Debug.LogError(request.Error.message);
            }
        }
    }
}
