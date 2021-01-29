using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCUIAPI
{
    public interface VCAPIInterface
    {
        string GetErrorMessage();
        /*
                bool IsVersionControlled(out bool yesNo, string unit = "");
                bool Login(string username, string password);
                bool AnythingUncommitted(out bool yesNo, VersionControlUnitInfo unit = null);
                bool GetRemoteCommits (int unitId, out List<CommitInfo> commits);
                bool GetRemoteUnits(int projectId, out List<VCUNIT> units);
                bool GET(int commitId, string alternativeName = "", string alternativePath = "");
                bool GetConflictedVersion(string conflictCommitId);
                bool PUSH(VersionControlUnitInfo unit, string message);
                bool DELETE(int commitId);
                bool GetReleases(int pid, out List<CommitInfo> versions);
                bool GET_VERSION(int projectId, int versionId, string alternativeName = "", string alternativePath = "", bool update = false);
                bool INTERRELATED_COMMIT(List<string> units);
                bool ChangeVersionControlledType(bool localVC, bool remoteVC);
                bool GetVersionControlContent(int projectId, out KeyValuePair<string, string> content);
                bool AddUnitToVersionControl(string unit, int unitType);
                bool RemoveUnitFromVersionControl(int projectId, int unitId);
                bool AddProjectToVersionControl(int parentId, string projectName);
                bool RemoveProjectFromVersionControl(int projectId);
                bool LinkLocalToVersionControl(int parentId);
                bool UnlinkLocalFromVersionControl();
                bool HasUserProjectAdminRight(int projectId, out bool yesNo);
                bool HasUserPushRight(int unitId, out bool yesNo);
                bool GetUsers(int unitId, out List<UserInfo> users);
                bool AddUsersToProject(int projectId, List<string> userIds, int accessLevel);
                bool RemoveUsersFromProject(int projectId, List<string> userIds);
                bool SetUserUnitRights(string userId, int defaultRight, Dictionary<int, int> rights);

                bool IsConflicted(out bool yesNo, VersionControlUnitInfo unit, out CommitInfo conflicted, out CommitInfo commonParentVersion);
                bool GetCurrentUser(out UserInfo userInfo);
                bool UnlinkUnitFromVersionControl(int unitId);
                bool LinkUnitToVersionControl(int unitId, out CommitInfo mostRecentCommit, bool failIfNoCommit = false);
                bool GetReleaseInfo(int projectId, int releaseId, out List<VersionControlUnitInfo> units, out List<CommitInfo> commits);
                bool GetCommonAncestor(out int ancestorCommitId, int unitId, int commitId1, int commitId2);
                bool PushRelease(string name, string message, List<VersionControlUnitInfo> units, List<CommitInfo> versions);
                bool IsCoreProject(out bool yesNo, out int coreType);
                bool HasUserProjectAdminRight(int projectId, string userId, out bool yesNo);
                bool SetUserProjectAdminRight(int projectId, string userId, bool yesNo);
        /**/
    }
}
