//EUROMOD GENERAL DEFINITIONS
#ifndef EMDEFINITIONS
	#define EMDEFINITIONS

	#define COMOD1 "func_arithop"
	#define COMOD2 "func_elig"
	#define COMOD3 "func_bencalc"
	#define COMOD4 "func_schedcalc"
	#define COMOD5 "func_minimaxi"
	#define COMOD6 "func_min" 
	#define COMOD7 "func_max"
	#define COMOD8 "func_allocate_f210" //!!! PHASE-OUT !!! see COMOD25
	#define COMOD9 "func_defconst"
	#define COMOD10 "func_defil"
	#define COMOD11 "func_deftu"
	#define COMOD12 "func_uprate"
	#define COMOD13 "func_defoutput"
	#define COMOD14 "func_updatetu"
	#define COMOD15 "func_changeparam"
	#define COMOD16 "func_loop"
	#define COMOD17 "func_unitloop"
	#define COMOD18 "func_store"
	#define COMOD19 "func_restore"
	#define COMOD20 "func_dropunit"
	#define COMOD21 "func_keepunit"
	#define COMOD22 "func_ilvarop"
	#define COMOD23 "func_totals"
	#define COMOD24 "func_defvar"
	#define COMOD25 "func_allocate"
	#define COMOD26 "func_randseed"
	#define COMOD27 "func_setdefault"
	#define COMOD28 "func_callprogramme"
	#define COMOD29 "func_definput"

	#define LAB_PARNAME "sys_par_name"
	#define LAB_FIRSTSYS "sys_first_sys"
	#define LAB_ENDSYS "sys_end_sys"
	#define LAB_FIRSTPAR "sys_first_par"
	#define LAB_ENDPAR "sys_end_par"
	#define LAB_PARID "sys_reserved"
	#define LAB_VARNAME "sys_var_name"
	#define LAB_MODGEN "sys_modgen"
	#define LAB_MON "sys_mon"
	#define LAB_DEFVAR "sys_defvar"
	#define LAB_DEFVAL "sys_defval"
	#define LAB_DEFAULT "sys_default"
	#define LAB_ENDVAR "sys_end_var"

	#define VARNAME_HHID "idhh"
	#define VARNAME_PID "idperson"
	#define VARNAME_PARTNERID "idpartner"
	#define VARNAME_PARENTID "idparent"
	#define VARNAME_MOTHERID "idmother"
	#define VARNAME_FATHERID "idfather"
	#define VARNAME_AGE "dag"
	#define VARNAME_MARITAL_STATUS "dms"
	#define VARNAME_LABSUP_STATUS "les"
	#define VARNAME_EDUC_STATUS "dec"
	#define VARNAME_DISAB "ddi"
	#define VARNAME_OCC_STATUS "loc"
	#define VARNAME_CIVIL_SERV "lcs"
	#define VARNAME_INTERNAL1 "internal1"
	#define VARNAME_INTERNAL2 "internal2"
	#define VARNAME_PREFIX_POLICY_SWITCH "ssw"

	#define EMVOID 0.0000000000001
	#define STREMVOID "0.0000000000001"
	#define POSINFINITE "999999999999.99"
	#define NEGINFINITE "-999999999999.99"
	#define NEGINFINITE_BR "(-999999999999.99)"
	#define NUMPOSINFINITE 999999999999.99
	#define NUMNEGINFINITE -999999999999.99

	#define ELIGTYPE_NOBODY 0
	#define ELIGTYPE_ALL 1
	#define ELIGTYPE_ALLADULTS 2
	#define ELIGTYPE_ONE 3
	#define ELIGTYPE_ONEADULT 4

	#define VITYPE_UNDEF 0
	#define VITYPE_VAR 1
	#define VITYPE_IL 2

	#define TUTYPE_HH 1
	#define TUTYPE_IND 2
	#define TUTYPE_SUBGROUP 3

	#define DEFAULT_EXTHEADCOND "(!{isdepchild})"
	#define DEFAULT_PARTNERCOND "({head:idperson=idpartner})"
	#define DEFAULT_DEPCHILDCOND "(!{isparent}&{idpartner<=0})"
	#define DEFAULT_NOCHILDCOND "{0}"
	#define DEFAULT_OWNCHILDCOND1 "({head:idperson=idparent}|{partner:idperson=idparent})"
	#define DEFAULT_OWNCHILDCOND2 "({head:idperson=idmother}|{head:idperson=idfather}|{partner:idperson=idmother}|{partner:idperson=idfather})"
	#define DEFAULT_OWNDEPCHILDCOND "({isownchild}&{isdepchild})"
	#define DEFAULT_LOOSEDEPCHILDCOND1 "({idparent=0}&{isdepchild})"
	#define DEFAULT_LOOSEDEPCHILDCOND2 "({idmother=0}&{idfather=0}&{isdepchild})"
	#define DEFAULT_PARENTCOND1 "(({head:idparent=idperson}|{partner:idparent=idperson})|({idpartner>0}&({head:idparent=idpartner}|{partner:idparent=idpartner})))"
	#define DEFAULT_PARENTCOND2 "({head:idmother=idperson}|{head:idfather=idperson}|{partner:idmother=idperson}|{partner:idfather=idperson})"
	#define DEFAULT_RELATIVECOND "({0})"
	#define DEFAULT_LONEPARENTCOND "({isparentofdepchild}&{idpartner<=0})"
	
	// Linux does not support the sprintf_s function, while sprintf is considered unsafe in Windows
	#ifdef _WIN32
		#define EM_SPRINTF sprintf_s
	#else
		#define EM_SPRINTF sprintf
	#endif
#endif
