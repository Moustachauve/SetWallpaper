using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCore.Command
{
	internal static class CommandSerializer
	{
		/// <summary>
		/// Serializes a command.
		/// </summary>
		/// <param name="pCommand">Command to be serialized</param>
		/// <returns>Byte array representing the command</returns>
		internal static byte[] SerializeCommand(Command pCommand) 
		{
			return pCommand.ToByteArray();
		}

		/// <summary>
		/// Deserializes a command.
		/// </summary>
		/// <param name="pData">Byte array representing the command</param>
		/// <returns>Deserialized command</returns>
		/// <exception cref="NetworkCore.Command.InvalidCommandException"></exception>
		internal static Command DeserializeCommand(byte[] pData)
		{
			CommandType type = (CommandType)pData[0];
			Command command = null;

			switch (type)
			{
				case CommandType.Notification:
					command = new NotificationCommand(pData);
					break;
				case CommandType.SetWallpaper:
					break;
				case CommandType.UserJoined:
					break;
				case CommandType.UserLeft:
					break;
				default:
					throw new InvalidCommandException();
			}

			return command;
		}

		/// <summary>
		/// Merges byte arrays.
		/// </summary>
		/// <param name="byteArrays">Arrays to merge</param>
		/// <returns>Result byte array</returns>
		internal static byte[] MergeByteArrays(params byte[][] byteArrays)
		{
			long arrayTotalSize = 0;
			for (long i = 0; i < byteArrays.LongLength; i++)
			{
				arrayTotalSize += byteArrays[i].Length;
			}

			byte[] resultArray = new byte[arrayTotalSize];
			int offset = 0;
			for(long i = 0; i < byteArrays.LongLength; i++)
			{
				Buffer.BlockCopy(byteArrays[i], 0, resultArray, offset, byteArrays[i].Length);
				offset += byteArrays[i].Length;
			}

			return resultArray;
		}

		internal static byte[] PrefixCommand(CommandType pCommand, byte[] commandData)
		{
			byte[] completeCommand = new byte[commandData.Length + 1];

			Buffer.BlockCopy(commandData, 0, completeCommand, 1, commandData.Length);

			completeCommand[0] = (byte)pCommand;

			return completeCommand;
		}
	}
}
