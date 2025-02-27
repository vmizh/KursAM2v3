﻿using ServiceStack.IO;
using ServiceStack.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;

namespace ServiceStack.VirtualPath
{
    public class ResourceVirtualDirectory : AbstractVirtualDirectoryBase
    {
        public static HashSet<string> EmbeddedResourceTreatAsFiles { get; set; } = new();

        protected Assembly backingAssembly;
        public string rootNamespace { get; set; }

        protected List<ResourceVirtualDirectory> SubDirectories;
        protected List<ResourceVirtualFile> SubFiles;

        public override IEnumerable<IVirtualFile> Files => SubFiles;

        public override IEnumerable<IVirtualDirectory> Directories => SubDirectories;

        public override string Name => DirectoryName;

        public string DirectoryName { get; set; }

        public override DateTime LastModified { get; }

        internal Assembly BackingAssembly => backingAssembly;

        public ResourceVirtualDirectory(IVirtualPathProvider owningProvider, 
            IVirtualDirectory parentDir, 
            Assembly backingAsm, 
            DateTime lastModified,
            string rootNamespace)
        : this(owningProvider, 
            parentDir, 
            backingAsm, 
            lastModified,
            rootNamespace,
            rootNamespace, 
            GetResourceNames(backingAsm, rootNamespace)) { }

        public ResourceVirtualDirectory(IVirtualPathProvider owningProvider, 
            IVirtualDirectory parentDir, 
            Assembly backingAsm, 
            DateTime lastModified,
            string rootNamespace, 
            string directoryName, 
            List<string> manifestResourceNames)
            : base(owningProvider, parentDir)
        {
            if (string.IsNullOrEmpty(directoryName))
                throw new ArgumentNullException(nameof(directoryName));

            this.backingAssembly = backingAsm ?? throw new ArgumentNullException(nameof(backingAsm));
            this.LastModified = lastModified;
            this.rootNamespace = rootNamespace;
            this.DirectoryName = directoryName;

            InitializeDirectoryStructure(manifestResourceNames);
        }

        public static List<string> GetResourceNames(Assembly asm, string basePath)
        {
            return asm.GetManifestResourceNames()
                .Where(x => x.StartsWith(basePath))
                .Map(x => x.Substring(basePath.Length).TrimStart('.'));
        }

        protected void InitializeDirectoryStructure(List<string> manifestResourceNames)
        {
            SubDirectories = new List<ResourceVirtualDirectory>();
            SubFiles = new List<ResourceVirtualFile>();

            SubFiles.AddRange(manifestResourceNames
                .Where(n => n.Count(c => c == '.') <= 1 || EmbeddedResourceTreatAsFiles.Contains(n))
                .Select(CreateVirtualFile)
                .Where(f => f != null)
                .OrderBy(f => f.Name));

            SubDirectories.AddRange(manifestResourceNames
                .Where(n => n.Count(c => c == '.') > 1)
                .GroupByFirstToken(pathSeparator: '.')
                .Select(CreateVirtualDirectory)
                .OrderBy(d => d.Name));
        }

        protected virtual ResourceVirtualDirectory CreateVirtualDirectory(IGrouping<string, string[]> subResources)
        {
            var remainingResourceNames = subResources.Select(g => g[1]);
            var subDir = new ResourceVirtualDirectory(
                VirtualPathProvider, this, backingAssembly, LastModified, rootNamespace, subResources.Key, remainingResourceNames.ToList());

            return subDir;
        }

        protected virtual ResourceVirtualFile CreateVirtualFile(string resourceName)
        {
            try
            {
                var fullResourceName = string.Concat(RealPath, VirtualPathProvider.RealPathSeparator, resourceName);

                var resourceNames = new[]
                {
                    fullResourceName,
                    fullResourceName.Replace(VirtualPathProvider.RealPathSeparator, ".").Trim('.')
                };

                var mrInfo = resourceNames.FirstOrDefault(x => backingAssembly.GetManifestResourceInfo(x) != null);
                if (mrInfo == null)
                {
                    LogManager.GetLogger(GetType()).Warn("Virtual file not found: " + fullResourceName);
                    return null;
                }

                return new ResourceVirtualFile(VirtualPathProvider, this, resourceName);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(GetType()).Warn(ex.Message, ex);
                return null;
            }
        }

        protected virtual ResourceVirtualDirectory ConsumeTokensForVirtualDir(Stack<string> resourceTokens)
        {
            var subDirName = resourceTokens.Pop();
            throw new NotImplementedException();
        }

        public override IEnumerator<IVirtualNode> GetEnumerator()
        {
            return Directories.Cast<IVirtualNode>().Union(Files.Cast<IVirtualNode>()).GetEnumerator();
        }

        protected override IVirtualFile GetFileFromBackingDirectoryOrDefault(string fileName)
        {
            var file = Files.FirstOrDefault(f => f.Name.EqualsIgnoreCase(fileName));
            if (file != null)
                return file;

            //ResourceDir reads /path/to/a.min.js as path.to.min.js and lays out as /path/to/a/min.js
            var parts = fileName.SplitOnFirst('.');
            if (parts.Length > 1)
            {
                if (GetDirectoryFromBackingDirectoryOrDefault(parts[0]) is ResourceVirtualDirectory dir)
                {
                    return dir.GetFileFromBackingDirectoryOrDefault(parts[1]);
                }
            }

            return null;
        }

        public string TranslatePath(string path) => path.Replace('-', '_');

        protected override IEnumerable<IVirtualFile> GetMatchingFilesInDir(string globPattern)
        {
            var useGlob = globPattern.TrimStart('/');
            var useGlobTranslate = TranslatePath(useGlob);
            return Files.Where(f => {
                return useGlob.IndexOf('/') >= 0
                    ? f.VirtualPath.Glob(useGlob) || f.VirtualPath.Glob(useGlobTranslate) 
                    : f.Name.Glob(useGlob) || f.Name.Glob(useGlobTranslate);
            });
        }

        protected override IVirtualDirectory GetDirectoryFromBackingDirectoryOrDefault(string directoryName)
        {
            return Directories.FirstOrDefault(d => d.Name.EqualsIgnoreCase(directoryName)) ??
                Directories.FirstOrDefault(d => d.Name.EqualsIgnoreCase(TranslatePath(directoryName ?? "")));
        }

        protected override string GetRealPathToRoot()
        {
            var path = base.GetRealPathToRoot();
            return path.TrimStart('.');
        }
    }
}
