using System;
using OpenMod.API.Commands;

namespace Dummy.Commands.Helpers;
internal sealed class CommandArgument
{
    private readonly string m_Name;
    private readonly string? m_ShortName;

    public CommandArgument(string name, char? shortName = null)
    {
        m_Name = name;
        if (!m_Name.StartsWith("--", StringComparison.OrdinalIgnoreCase))
        {
            m_Name = "--" + m_Name;
        }

        if (shortName != null)
        {
            m_ShortName = "-" + shortName;
        }
    }

    public string? GetArgument(ICommandParameters parameters)
    {
        for (var i = 0; i < parameters.Count - 1; i += 2)
        {
            var key = parameters[i];
            var value = parameters[i + 1];

            if (key.Equals(m_Name, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            if (m_ShortName != null && key.Length > 0 && key.Equals(m_ShortName, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
        }

        return null;
    }
}
