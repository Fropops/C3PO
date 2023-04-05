using System.Collections.Generic;
using TeamServer.Services;
using ApiModels.Changes;

public interface IChangeTrackingService
{
    List<Change> ConsumeChanges(string session);
    void CleanSession(string session);
    void RecordSession(string session);

    void TrackChange(ChangingElement element, string id);
}

public class ChangeTrackingService : IChangeTrackingService
{
    public Dictionary<string, List<Change>> TrackedChanges = new Dictionary<string, List<Change>>();

    public void TrackChange(ChangingElement element, string id)
    {
        foreach(var session in this.TrackedChanges.Keys)
        {
            var change = new Change(element, id);
            this.TrackedChanges[session].Add(change);
        }
    }

    public List<Change> ConsumeChanges(string session)
    {
        if (!this.TrackedChanges.ContainsKey(session))
        {
            this.TrackedChanges.Add(session, new List<Change>());
            return new List<Change>();
        }
        
        var lst = this.TrackedChanges[session];
        this.TrackedChanges[session] = new List<Change>();
        return lst;
    }

    public void CleanSession(string session)
    {
        if (!this.TrackedChanges.ContainsKey(session))
            return;
        this.TrackedChanges.Remove(session);
    }

    public void RecordSession(string session)
    {
        if (this.TrackedChanges.ContainsKey(session))
            return;
        this.TrackedChanges.Add(session, new List<Change>());
    }
}

