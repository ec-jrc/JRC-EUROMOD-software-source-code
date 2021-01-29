using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;


namespace VCUIAPI
{
    [XmlRoot()]
    public class VersionControlProjectInfo
    {
        internal bool isConnected;

        [XmlAttribute]
        public long ProjectId { get; set; }

        [XmlAttribute]
        public long FolderAddOnsId { get; set; }

        [XmlAttribute]
        public long FolderCountriesId { get; set; }

        [XmlAttribute]
        public long FolderConfigId { get; set; }

        [XmlAttribute]
        public long FolderLogId { get; set; }

        [XmlAttribute]
        public long FolderInputId { get; set; }

        [XmlAttribute]
        public string ProjectName { get; set; }

        [XmlAttribute]
        public string Description { get; set; }

        [XmlArray]
        public List<VersionControlUnitInfo> Units { get; set; }

        public VersionControlUnitInfo GetUnit(string name, VCAPI.VC_FOLDER_TYPE unitType)
        {
            try { 
                return Units.Find(x => x.Name.ToLower() == name.ToLower() && x.UnitType == unitType); 
            } catch { return null; }
        }

        public VersionControlUnitInfo GetUnit(long unitId)
        {
            try { return Units.Find(x => x.UnitId == unitId); } catch { return null; }
        }
    }

    public class VersionControlUnitInfo
    {
        public string Version { get; set; }
        public string UnitHash { get; set; }

        public long ProjectId { get; set; }

        public long UnitId { get; set; }

        public string Name { get; set; }

        public VCAPI.VC_FOLDER_TYPE UnitType { get; set; }
    }  
    
    /// <summary>
    /// Information of a given commit.
    /// </summary>
    public class ReleaseInfo
    {
        public long Id { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
    };

    /// <summary>
    /// Information on a particular user (usually used as out parameter to retrieve respective info).
    /// </summary>
    public class UserInfo   
    {
        public long userId { get; set; }
        public string username { get; set; } = "";
        public string password { get; set; } = "";
        public string firstName { get; set; } = "";
        public string lastName { get; set; } = "";
        public string email { get; set; } = "";
        public string forgotPasswordQuestion { get; set; } = "";
        public string forgotPasswordAnswer { get; set; } = "";
    };

    /// <summary>
    /// A class that holds a tree of all available Version Control projects
    /// </summary>
    public class ProjectNode
    {
        private readonly VCAPI.VC_FOLDER_TYPE type;
        private readonly long id;
        private long parentId;
        private readonly string name;
        private readonly string description;
        private readonly string hash;
        private readonly string sha;
        private readonly List<ProjectNode> children = new List<ProjectNode>();

        public long Id { get { return id; } }
        public long ParentId { get { return parentId; } }
        public VCAPI.VC_FOLDER_TYPE Type { get { return type; } }
        public string Name { get { return name; } }
        public string Description { get { return description; } }
        public String Hash { get { return hash; } }
        public String Sha { get { return sha; } }

        public List<ProjectNode> Children { get { return children; } }

        public ProjectNode(VCAPI.VC_FOLDER_TYPE _type, long _id, string _name, long _parentId, string _description, string _hash, string _sha)
        {
            type = _type;
            id = _id;
            name = _name;
            parentId = _parentId;
            description = _description;
            hash = _hash;
            sha = _sha;
        }

        public void addChild(ProjectNode node)
        {
            node.parentId = id;
            children.Add(node);
        }

        public void removeChild(long id)
        {
            foreach (ProjectNode child in Children)
            {
                if (child.id == id)
                {
                    Children.Remove(child);
                    return;
                }
                else
                {
                    child.removeChild(id);
                }
            }
        }

        public ProjectNode getNodeById(long searchId)
        {
            if (id == searchId) return this;
            else
            {
                foreach (ProjectNode child in Children)
                {
                    ProjectNode res = child.getNodeById(searchId);
                    if (res != null) return res;
                }
            }
            return null;
        }

        public List<ProjectNode> GetFlatProjectList()
        {
            List<ProjectNode> nodes = new List<ProjectNode>();
            if (type == VCAPI.VC_FOLDER_TYPE.FOLDER || type == VCAPI.VC_FOLDER_TYPE.PROJECT)
            {
                if (type == VCAPI.VC_FOLDER_TYPE.PROJECT) nodes.Add(this);
                else foreach (ProjectNode child in children) nodes.AddRange(child.GetFlatProjectList());
            }
            return nodes;
        }
    }

    public class Notification
    {
        public Guid Id { get; set; }
        public short EventType { get; set; }
        public string EventDescription { get; set; }
        public string Message { get; set; }
        public long ProjectId { get; set; }
        public long SubjectId { get; set; }
        public List<long> Recipients { get; set; }
        public List<short> DispatchMode { get; set; }
    }
}
