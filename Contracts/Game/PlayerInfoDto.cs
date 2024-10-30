using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominex.Contracts.Game;
public class PlayerInfoDto
{
	public string GamePhase { get; set; } // todo enum
	public int Actions { get; set; }
	public int Buys { get; set; }
	public int Coins { get; set; }
}
