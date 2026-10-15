using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal static class EditorEventFilters
    {
        internal static EditorEvent QueryHelper(this EditorEvent item, Rect connection = default(Rect))
        {
            if (!item.m_TagPolicy)
            {
                return item;
            }

            if (connection == default(Rect))
            {
                connection = GUILayoutUtility.GetLastRect();
            }

            item.m_TagPolicy = connection.Contains(item.m_RegistryPolicy.mousePosition);
            return item;
        }

        internal static EditorEvent InvokeHelper(this EditorEvent config)
        {
            while (true)
            {
                if (config.m_TagPolicy)
                {
                    int num = 1;
                    while (true)
                    {
                        switch (num)
                        {
                            default:
                                return config;
                            case 2:
                                break;
                            case 5:
                                goto end_IL_0041;
                            case 1:
                                config.m_TagPolicy = config.m_RegistryPolicy.type == EventType.MouseDown;
                                num = 4;
                                continue;
                            case 3:
                                goto IL_00f9;
                        }

                        break;
                    }

                    continue;
                }

                goto IL_00f9;
                IL_00f9:
                    ;
                break;
                continue;
                end_IL_0041:
                    break;
            }

            return config;
        }

        internal static EditorEvent ExcludeHelper(this EditorEvent first)
        {
            while (true)
            {
                if (first.m_TagPolicy)
                {
                    int num = 3;
                    while (true)
                    {
                        switch (num)
                        {
                            case 4:
                                break;
                            case 5:
                                return first;
                            case 1:
                            case 3:
                                first.m_TagPolicy = first.m_RegistryPolicy.button == 0;
                                num = 5;
                                continue;
                            case 2:
                                goto end_IL_003f;
                            default:
                                goto IL_00be;
                        }

                        break;
                    }

                    continue;
                }

                goto IL_00be;
                IL_00be:
                    ;
                break;
                continue;
                end_IL_003f:
                    break;
            }

            return first;
        }
    }
}
