using Telegram.Bot.Types;

namespace WistBot
{
    internal class ListObject
    {
        public string Name;
        public string? Description;
        public string? Link;
        public Document? Document;
        public PhotoSize[]? Photo;
        public User? Performer;
        public enum State { free, busy, done };
        private State state;

        public ListObject(string name, string? desc = null, string? link = null, Document? doc = null, PhotoSize[]? photo = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            }
            Name = name;
            Description = desc;
            Link = link;
            Document = doc;
            Photo = photo;
            state = State.free;
        }

        public void SetState(State newState)
        {
            if (newState == state)
            {
                return;
            }
            state = newState;
        }

        public void SetPerformer(Telegram.Bot.Types.User newPerformer)
        {
            if (newPerformer != null)
            {
                Performer = newPerformer;
            }
        }

        public void SetName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) 
            {
                return;
            }
            Name = newName;
        }

        public void SetDescription(string newDesc)
        {
            if (string.IsNullOrWhiteSpace(newDesc))
            {
                return;
            }
            Description = newDesc;
        }

        public void SetLink(string newLink)
        {
            if (string.IsNullOrWhiteSpace(newLink))
            {
                return;
            }
            if (!Uri.IsWellFormedUriString(newLink, UriKind.Absolute))
            {
                return;
            }
            Link = newLink;
        }

        public void SetDocument(Document newDoc)
        {
            if (newDoc != null)
            {
                Document = newDoc;
            }
        }

        public void SetPhoto(PhotoSize[] newPhoto)
        {
            if (newPhoto != null)
            {
                Photo = newPhoto;
            }
        }

        public string GetState()
        {
            return state.ToString();
        }
    }
}
