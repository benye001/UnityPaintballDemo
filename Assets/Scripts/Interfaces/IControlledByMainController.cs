using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Ensures that player components are initialized & ticked after PlayerMainController, & that they have a way to go back to the default state, for respawning purposes
/// </summary>
public interface IControlledByMainController
{
	void Initialize();
	void Tick();
	void ResetToDefaults();
}
