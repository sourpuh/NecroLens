using Dalamud.Plugin;

namespace Pictomancy;

public class Pipe<E> : IDisposable
{
    private DalamudPluginInterface plugin;
    private string tag;
    //private List<E> elements;

    public Pipe(DalamudPluginInterface plugin, string elementName, bool reader)
    {
        this.plugin = plugin;
        this.tag = "SplatoonApi." + elementName;

        if (reader)
            Reader = new PipeReader(this);
        else
            Writer = new PipeWriter(this);
    }

    public PipeReader Reader { get; private set; }

    public PipeWriter Writer { get; private set; }
    public void Dispose()
    {
        Reader?.Dispose();
        Writer?.Dispose();
    }

    public class PipeReader : IDisposable
    {
        private Pipe<E> pipe;
        private List<E> data;

        internal PipeReader(Pipe<E> pipe)
        {
            this.pipe = pipe;
            data = pipe.plugin.GetOrCreateData<List<E>>(pipe.tag, () => []);
        }

        public IEnumerable<E> Read()
        {
            // pipe.chat.Print(pipe.tag + ".Read " + data.Count);
            foreach (E data in data)
                yield return data;
            data.Clear();
        }

        public void Dispose()
        {
            pipe.plugin.RelinquishData(pipe.tag);
        }
    }
    public class PipeWriter : IDisposable
    {
        private Pipe<E> pipe;
        private List<E> data;


        internal PipeWriter(Pipe<E> pipe)
        {
            this.pipe = pipe;
            this.data = new();
            pipe.plugin.TryGetData(pipe.tag, out this.data);
        }

        public void Write(E data)
        {
            //pipe.chat.Print(pipe.tag + ".Write " + data);
            this.data.Add(data);
        }

        public void Flush()
        {
            /*
            //pipe.chat.Print(pipe.tag + ".Flush");
            if (pipe.plugin.TryGetData(pipe.tag, out this.data))
            {
                //pipe.chat.Print(pipe.tag + ".Add");
                data.AddRange(this.data);
            }
            this.data.Clear();
            */
        }

        public void Dispose()
        {
            pipe.plugin.RelinquishData(pipe.tag);
        }
    }
}
