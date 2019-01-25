// Copyright 1998-2018 Epic Games, Inc. All Rights Reserved.

#include "NightFightersPlayerController.h"
#include "AI/Navigation/NavigationSystem.h"
#include "Runtime/Engine/Classes/Components/DecalComponent.h"
#include "HeadMountedDisplayFunctionLibrary.h"
#include "NightFightersCharacter.h"
#include "Engine/World.h"
#include "GameFramework/CharacterMovementComponent.h"

ANightFightersPlayerController::ANightFightersPlayerController()
{
	bShowMouseCursor = true;
	bMoveToMouseCursor = true;
	direction = FVector(0, 1, 0);
	speed = 5000;
	DefaultMouseCursor = EMouseCursor::Crosshairs;
}

void ANightFightersPlayerController::PlayerTick(float DeltaTime)
{
	Super::PlayerTick(DeltaTime);

	// keep updating the destination every tick while desired
	if (bMoveToMouseCursor)
	{
		MoveToMouseCursor();
	}
}

void ANightFightersPlayerController::SetupInputComponent()
{
	// set up gameplay key bindings
	Super::SetupInputComponent();

	//InputComponent->BindAction("SetDestination", IE_Pressed, this, &ANightFightersPlayerController::OnSetDestinationPressed);
	//InputComponent->BindAction("SetDestination", IE_Released, this, &ANightFightersPlayerController::OnSetDestinationReleased);
	InputComponent->BindAction("PrimaryPressed", IE_Pressed, this, &ANightFightersPlayerController::OnPrimaryPressed);

	// support touch devices 
	InputComponent->BindTouch(EInputEvent::IE_Pressed, this, &ANightFightersPlayerController::MoveToTouchLocation);
	InputComponent->BindTouch(EInputEvent::IE_Repeat, this, &ANightFightersPlayerController::MoveToTouchLocation);

	InputComponent->BindAction("ResetVR", IE_Pressed, this, &ANightFightersPlayerController::OnResetVR);
}

void ANightFightersPlayerController::OnResetVR()
{
	UHeadMountedDisplayFunctionLibrary::ResetOrientationAndPosition();
}

void ANightFightersPlayerController::MoveToMouseCursor()
{
	/*
	if (UHeadMountedDisplayFunctionLibrary::IsHeadMountedDisplayEnabled())
	{
		if (ANightFightersCharacter* MyPawn = Cast<ANightFightersCharacter>(GetPawn()))
		{
			if (MyPawn->GetCursorToWorld())
			{
				UNavigationSystem::SimpleMoveToLocation(this, MyPawn->GetCursorToWorld()->GetComponentLocation());
			}
		}
	}
	else
	{
		// Trace to see what is under the mouse cursor
		FHitResult Hit;
		GetHitResultUnderCursor(ECC_Visibility, false, Hit);

		if (Hit.bBlockingHit)
		{
			// We hit something, move there
			SetNewMoveDestination(Hit.ImpactPoint);
		}
	
	*/
	FHitResult Hit;
	GetHitResultUnderCursor(ECC_Visibility, false, Hit);

	// We hit something, move there
	FVector newDirection = Hit.ImpactPoint - GetPawn()->GetActorLocation();
	newDirection = FVector(newDirection.X, newDirection.Y, 0);
	float distToMouse = newDirection.Size();
	newDirection.Normalize();
	direction = newDirection;

	GetCharacter()->SetActorRotation(direction.ToOrientationRotator());
	if (Hit.bBlockingHit && distToMouse > 120.0f)
	{
		GetCharacter()->GetCharacterMovement()->AddInputVector(direction * speed);

	}
}

void ANightFightersPlayerController::MoveToTouchLocation(const ETouchIndex::Type FingerIndex, const FVector Location)
{
	FVector2D ScreenSpaceLocation(Location);

	// Trace to see what is under the touch location
	FHitResult HitResult;
	GetHitResultAtScreenPosition(ScreenSpaceLocation, CurrentClickTraceChannel, true, HitResult);
	if (HitResult.bBlockingHit)
	{
		// We hit something, move there
		SetNewMoveDestination(HitResult.ImpactPoint);
	}
}

void ANightFightersPlayerController::SetNewMoveDestination(const FVector DestLocation)
{
	APawn* const MyPawn = GetPawn();
	if (MyPawn)
	{
		UNavigationSystem* const NavSys = GetWorld()->GetNavigationSystem();
		float const Distance = FVector::Dist(DestLocation, MyPawn->GetActorLocation());

		// We need to issue move command only if far enough in order for walk animation to play correctly
		if (NavSys && (Distance > 120.0f))
		{
			NavSys->SimpleMoveToLocation(this, DestLocation);
		}
	}
}

void ANightFightersPlayerController::OnSetDestinationPressed()
{
	// set flag to keep updating destination until released
	bMoveToMouseCursor = true;
}

void ANightFightersPlayerController::OnSetDestinationReleased()
{
	// clear flag to indicate we should stop updating the destination
	bMoveToMouseCursor = false;
}

void ANightFightersPlayerController::OnPrimaryPressed()
{
	//std::cout << "OnPrimaryPressed" << std::endl;
	//GetCharacter()->GetCharacterMovement()->StopMovementImmediately();
	//GetCharacter()->GetCharacterMovement()->AddImpulse(direction * 10000);
}

void ANightFightersPlayerController::OnPrimaryReleased()
{
}
