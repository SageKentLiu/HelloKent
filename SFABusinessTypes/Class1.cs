using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFABusinessTypes
{
    public enum bpITCTypeEnum
    {
		NoITC=0,
		NewPropFullCredit,
		NewPropReducedCredit,
		UsedPropFullCredit,
		UsedPropReducedCredit,
		Rehab30Year,
		Rehab40Year,
		CertHistoricRehab,
		NonCertHistoricRehab,
		Biomass,
		IntercityBuses,
		HydroelectricGenerating,
		OceanThermal,
		SolarEnergy,
		Wind,
		GeoThermal,
		CertHistoricTransition,
		QualifiedProgressExp,
		Reforestation,
		SolarEnergyProperty,
		OtherEnergyProperty,
		FuelCellProperty,
		MicroturbineProperty,
		AdvancedCoalProject,
		GasificationProject,
		HeatPowerSystem,
		SmallWindEnergy,
		GeothermalHeatPump,
		AdvancedEnergyProject,
		UnknownITCType // internal use only
	} ;

    public enum bpBookTypeEnum
	{
		TaxBook,
		InternalBook,
		StateBook,
		AMTBook,
		EandPBook,
		ACEBook,
		UserBook
	} ;

    public enum bpDeprSwitchTypeEnum
	{
		SwitchWhenOptimal=1,
		DontSwitch,
		UnknownSwitch, // internal use only.
		MidQtrSwitch
	} ;

    public enum bpAssetActivityTypeEnum
	{
		Active=1,               // 'A'
		Inactive,               // 'I'
		Disposed,               // 'D'
		WholeTransferDisposed,  // 'F'
		PartiallyDisposed,      // 'J'
		WholeTransfer,          // 'K'
		PartialTransferWithinCmp,  // 'L'
		PartialTransferOutsideCmp, // 'M'
		PartialTransferDisposed,   // 'N'
		ADIImport                  // 'X'
	} ;

    public class Class1
    {
    }
}
