using System.Runtime.CompilerServices;

namespace FynnsTools.ControllerEditor
{
    internal sealed class ToolVersion
    {
        [CompilerGenerated]
        private readonly int factoryServer;
        [CompilerGenerated]
        private readonly int m_AccountServer;
        [CompilerGenerated]
        private readonly int m_RefServer;
        [SpecialName]
        [CompilerGenerated]
        internal int TestConnection()
        {
            return factoryServer;
        }

        [SpecialName]
        [CompilerGenerated]
        internal int ValidateConnection()
        {
            return m_AccountServer;
        }

        [SpecialName]
        [CompilerGenerated]
        internal int RateConnection()
        {
            return m_RefServer;
        }

        internal ToolVersion(int previous_first, int col_Position, int maxtag)
        {
            factoryServer = previous_first;
            m_AccountServer = col_Position;
            m_RefServer = maxtag;
        }

        internal ToolVersion(string res)
        {
            int num = 1;
            string[] array = default(string[]);
            while (true)
            {
                switch (num)
                {
                    default:
                        return;
                    case 0:
                        return;
                    case 2:
                        m_RefServer = int.Parse(array[2]);
                        num = 0;
                        break;
                    case 4:
                        m_AccountServer = int.Parse(array[1]);
                        num = 2;
                        break;
                    case 3:
                        factoryServer = int.Parse(array[0]);
                        num = 4;
                        break;
                    case 1:
                        array = res.Split(new char[1]{'.'});
                        num = 3;
                        break;
                }
            }
        }

        public static bool operator>(ToolVersion init, ToolVersion ord)
        {
            if (init.TestConnection() > ord.TestConnection())
            {
                return true;
            }

            if (init.TestConnection() >= ord.TestConnection())
            {
                if (init.ValidateConnection() <= ord.ValidateConnection())
                {
                    if (init.ValidateConnection() < ord.ValidateConnection())
                    {
                        return false;
                    }

                    return init.RateConnection() > ord.RateConnection();
                }

                return true;
            }

            return false;
        }

        public static bool operator <(ToolVersion task, ToolVersion vis)
        {
            return vis > task;
        }

        public static bool operator >=(ToolVersion item, ToolVersion connection)
        {
            return !(item < connection);
        }

        public static bool operator <=(ToolVersion last, ToolVersion ord)
        {
            return !(last > ord);
        }

        public static bool operator ==(ToolVersion res, ToolVersion col)
        {
            while (res.TestConnection() == col.TestConnection())
            {
                int num = 3;
                while (true)
                {
                    switch (num)
                    {
                        case 4:
                            break;
                        default:
                            goto end_IL_0064;
                        case 2:
                            if (res.ValidateConnection() != col.ValidateConnection())
                            {
                                num = 1;
                                continue;
                            }

                            return res.RateConnection() == col.RateConnection();
                        case 3:
                            goto case 2;
                    }

                    break;
                }

                continue;
                end_IL_0064:
                    break;
            }

            return false;
        }

        public static bool operator !=(ToolVersion config, ToolVersion ivk)
        {
            return !(config == ivk);
        }

        public override bool Equals(object param)
        {
            if (this != param)
            {
                if (param is ToolVersion singletonServer)
                {
                    return this == singletonServer;
                }

                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return (((TestConnection() * 397) ^ ValidateConnection()) * 397) ^ RateConnection();
        }

        public override string ToString()
        {
            return $"{TestConnection()}.{ValidateConnection()}.{RateConnection()}";
        }

        public static implicit operator ToolVersion(string asset)
        {
            return new ToolVersion(asset);
        }
    }
}
