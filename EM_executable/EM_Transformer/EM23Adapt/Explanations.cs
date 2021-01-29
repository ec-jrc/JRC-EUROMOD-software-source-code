// ***********************************************************************************************************************************
// ****************************************************  E X P L A N A T I O N S  ****************************************************
// ***********************************************************************************************************************************

// -----------------------------------------------------------------------------------------------------------------------------------
// EXPLANATION WRT TO NEW HANDLING OF REFERENCE POLICIES
// the purpose of reference policies in EM2 was to mark in the spine where the policy is repeated, i.e. providing the 'order'
// in EM3 the 'order' is provided by <SysPol> which links the policy
// thus it's easily possible to just link the policy twice (or n-times), i.e. have two (or more) <SysPol>s with different 'order'
// but in order to not loose the reference-policy's id, which may be used by e.g. ChangeSwitch, we keep the link between pol-id and refpol-id
//
// example:
// <Policy> <ID>pol-twice-in-the-spine</ID> ... </Policy>
// <Function> <PolicyID>pol-twice-in-the-spine</PolicyID> <ID>fun-of-pol-twice-in-the-spine</ID> ... </Function>
// <Parameter> <FunctionID>fun-of-pol-twice-in-the-spine</FunctionID> <ID>par-of-fun-of-pol-twice-in-the-spine</ID> ... </Parameter>
//
// * 1st reference *: <SysPol> <PolicyID>pol-twice-in-the-spine</PolicyID> <SysID>id-sys-1</SysID> <Order>3</Order> ... </SysPol> 
// * 2nd reference *: <SysPol> <PolicyID>pol-twice-in-the-spine</PolicyID> <SysID>id-sys-1</SysID> <Order>9</Order> ... </SysPol>
// (orders in other systems can be different or equal)
//
// functions and parameters are the same, thus only one reference (in old and new version)
// <SysFun> <FunctionID>fun-of-pol-twice-in-the-spine</FunctionID> <SysID>id-sys-1</SysID> <Order>1</Order> ... </SysFun>
// <SysPar> <ParameterID>par-of-fun-of-pol-twice-in-the-spine</ParameterID> <SysID>id-sys-1</SysID> <Order>1</Order> ... </SysPar>
//
// the links are kept like this:
// <REFPOL> <ID>id-of-the-reference</ID> <RefPolID>id-of-the-referred-policy</RefPolID> </REFPOL>
// -----------------------------------------------------------------------------------------------------------------------------------

// -----------------------------------------------------------------------------------------------------------------------------------
// EXPLANATION WRT TO NEW HANDLING OF CHANGEPARAM
// - parameter Param_CondVal is abolished (i.e. no run-time changes anymore, but see ChangeSwitch below),
//   respectively renamed to Param_NewVal where the change actually concerns a read-time-change
// - parameter Run_Cond is abolished, respectively replaced by:
// - parameter Dataset (non-unique): usage like in Uprate/SetDefault, except, not provided means apply for any dataset here
// examples: Dataset = sk_2019, Param_Id = 9ac9d5e6-bbfe-4e5c-a87e-940b33a5457e, Param_NewVal = adp*0.5 (e.g. if adp exists only in this data)
//           (in an AddOn:) Param_Id = yem_sk_#2.1, Param_NewVal = { dag <= 6 }
// run time changes are restricted to switching on/off policies/functions, which is accomplished by a new function:
// ChangeSwitch, with the following parameters:
// - parameter-group: PolFun: (symbolic) id of policy/function, NewSwitch: on/off
// - optional parameter Run_Cond (if present: run-time switch-change, if not present: read-time switch-change)
// examples: Run_Cond = { $ScenarioX = 1 }, PolFun (Group 1) = bsa_cy, NewSwitch (Group 1) = off,
//                                          PolFun (Group 2) = A18317A2-25AC-4E58-AFA2-92E80623550F, NewSwitch (Group 2) = on
//           (in an AddOn:) Run_Cond = { LoopCount_XYZ = 3 }, PolFun (Group 1) = tco_cy, NewSwitch (Group 1) = off,
//                                                            PolFun (Group 2) = tin_cy#1, NewSwitch (Group 2) = off
// -----------------------------------------------------------------------------------------------------------------------------------

// -----------------------------------------------------------------------------------------------------------------------------------
// EXPLANATION WRT TO TOGGLE AND SWITCH
// SWITCH:
// in EM2 the switch of switchable policies is literally set to 'switch' (e.g. BTA_CY = switch)
// in EM3 they are set to "off", (not to "n/a" because this is removed by the XMLHandler)
// for setting the actual switch see 'explanation wrt new handling of policy switches' below
// TOGGLE:
// the purpose of setting policies and functions to 'toggle' in EM2 was to initially switch them off and later on (usually by an AddOn)
// simply switching off was not possible because the old executable deleted switched-off policies on reading
// with the new order of procedures in EM3 one can set such policies/functions to off without having them deleted (too early)
// -----------------------------------------------------------------------------------------------------------------------------------

// -----------------------------------------------------------------------------------------------------------------------------------
// EXPLANATION WRT NEW HANDLING OF POLICY SWITCHES
// EM2 had(sic) "switchable policies", i.e. globally defined switches, e.g. BTA_??, which make policies named BTA_at, ... BTA_uk switchable
// EM3 (and latest EM2) have "extensions" which allow for "switch groups", i.e. one or more policies and functions which are switchable as a group
// technical changes:
// global file:
// - SwitchablePolicyConfig.xml renamed to Extensions.xml
// - <NamePattern> renamed to <ShortName> and removal of wildcards e.g. BTA_?? -> BTA
//   reason: in future this will not be used to identify the switchable policy in the countries anymore (see above)
//   but just as a short-name for e.g. display in the run-tool
//   note: automatic removal of wildcards is outcommeted, as it is confusing and should be done be developers manually
// - <LongName> renamed to <Name> (just cosmetic)
// country files:
// - <PolicySwitch> in cc_DataConfig.xlm moved to <Extension_Switch> in cc.xml
// - change from EM2 to EM3 format:
//   EM2:<PolicySwitch> <SwitchablePolicyID/> <SystemID/> <DataBaseID/> <Value/> </PolicySwitch> (of course not with empty attributes)
//   EM3: <Extension_Switch> <ExtensionID/> <SysID/> <DataID/> </Value> </Extension_Switch>
//   where <ExtensionID/> = <SwitchablePolicyID/>, <SysID/> = <SystemID/>, <DataID/> = <DataBaseID/>
//   the currenly only content of the extension, i.e. the id of the former switchable policy (in the resp.system) forms an item in
//   the <EXTENSION_POLs>-group as <EXTENSION_POL> <ExtensionID/> </PolID> </BaseOff> </EXTENSION_POL>
// - <PolicySwitch>es for which <SwitchID/> or </PolicyID> cannot be found are removed (probably out-dated, i.e. error correction)
// - <PolicySwitch>es set to n/a are removed (does only make sense for the UI)
// - cosmetic: in EM2 value is enclosed by CDATA - that's unnecessary and therefore removed (i.e. <![CDATA[off]]> -> off)
// - abolished value 'switch' for policy/function-switches (see "explanation wrt to toggle and switch" above)
// -----------------------------------------------------------------------------------------------------------------------------------
