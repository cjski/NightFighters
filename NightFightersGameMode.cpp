// Copyright 1998-2018 Epic Games, Inc. All Rights Reserved.

#include "NightFightersGameMode.h"
#include "NightFightersPlayerController.h"
#include "NightFightersCharacter.h"
#include "UObject/ConstructorHelpers.h"

ANightFightersGameMode::ANightFightersGameMode()
{
	// use our custom PlayerController class
	PlayerControllerClass = ANightFightersPlayerController::StaticClass();

	// set default pawn class to our Blueprinted character
	static ConstructorHelpers::FClassFinder<APawn> PlayerPawnBPClass(TEXT("/Game/TopDownCPP/Blueprints/TopDownCharacter"));
	if (PlayerPawnBPClass.Class != NULL)
	{
		DefaultPawnClass = PlayerPawnBPClass.Class;
	}
}