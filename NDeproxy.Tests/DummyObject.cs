

namespace NDeproxy.Tests
{
    class DummyObject
    {
        int id;

        public override string ToString()
        {
            return string.Format("dummy object, id = {0}", id);
        }
    }
}