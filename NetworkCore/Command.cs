using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCore
{
    internal enum CommandType : byte
    {
        Notification = 0,
        Wallpaper
    }

    internal static class Command
    {

        internal static byte[] PrefixCommand(CommandType pCommand, byte[] commandData)
        {
            byte[] completeCommand = new byte[commandData.Length + 1];

            Array.Copy(commandData, 0, completeCommand, 1, commandData.Length);

            completeCommand[0] = (byte)pCommand;

            return completeCommand;
        }

        internal static byte[] PrefixCommand(CommandType pCommand, string pParameters)
        {
            byte[] completeCommand = new byte[pParameters.Length + 1];
            Encoding.UTF8.GetBytes(pParameters, 0, pParameters.Length, completeCommand, 1);

            completeCommand[0] = (byte)pCommand;

            return completeCommand;
        }

        internal static Tuple<CommandType, string> ToString(byte[] pCommand)
        {
            var commandType = (CommandType)pCommand[0];
            string parameters = Encoding.UTF8.GetString(pCommand, 1, pCommand.Length - 1);

            return new Tuple<CommandType, string>(commandType, parameters);
        }
    }
}
