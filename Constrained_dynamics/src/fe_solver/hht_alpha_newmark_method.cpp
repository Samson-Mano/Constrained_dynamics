#include "hht_alpha_newmark_method.h"

hht_alpha_newmark_method::hht_alpha_newmark_method()
{
	// Empty constructor

}

const Eigen::VectorXd& hht_alpha_newmark_method::get_displacement_at_t() const
{
	return this->displ_at_t;
}

const Eigen::VectorXd& hht_alpha_newmark_method::get_velocity_at_t() const
{
	return this->velo_at_t;
}

const Eigen::VectorXd& hht_alpha_newmark_method::get_acceleration_at_t() const
{
	return this->accl_at_t;
}



void hht_alpha_newmark_method::initialize_hhtsolver(const Eigen::MatrixXd& massMatrix,
	const Eigen::MatrixXd& inversemassMatrix,
	const Eigen::MatrixXd& stiffnessMatrix,
	const Eigen::MatrixXd& dampingMatrix,
	const Eigen::VectorXd& initialDispl,
	const Eigen::VectorXd& initialVelo,
	const Eigen::VectorXd& initialForce,
	int solvertype)
{
	// Set the matrix
	this->massMatrix = massMatrix.sparseView();
	this->stiffnessMatrix = stiffnessMatrix.sparseView();
	this->dampingMatrix = dampingMatrix.sparseView();

	this->displ_at_t = initialDispl;
	this->velo_at_t = initialVelo;

	this->force_vector_at_t = initialForce;
	this->force_vector_prev = initialForce;


	// Find the acceleration at time t
	Eigen::VectorXd dampCVelo = dampingMatrix * initialVelo;
	Eigen::VectorXd stiffKDispl = stiffnessMatrix * initialDispl;

	this->accl_at_t = inversemassMatrix * (initialForce - dampCVelo - stiffKDispl);

	this->solvertype = solvertype;

	if (solvertype == 0)
	{
		// HHT Solver
		gamma = 0.5 * (1.0 - 2.0 * alpha);
		beta = 0.25 * (1.0 - alpha) * (1.0 - alpha);
	}
	else if (solvertype == 1)
	{
		// Newmark solver (linear acceleration)
		gamma = 0.5;
		beta = 1.0 / 6.0;
	}

}

void hht_alpha_newmark_method::hht_alpha_newmark_solve(double dt)
{
	if (solvertype == 0)
	{
		hht_solve(dt);
	}
	else if (solvertype == 1)
	{
		newmark_solve(dt);
	}
	//
}


void hht_alpha_newmark_method::hht_solve(double dt)
{
	// Note: The force is updated before the solve step is called

	// Find the time step factors
	double inv_beta = 1.0 / this->beta;
	double inv_2beta = 1.0 / (2.0 * this->beta);
	double gamma_invbeta = inv_beta * this->gamma;
	double gamma_2invbeta = inv_2beta * this->gamma;

	// Find the factor a1, a2, a3
	double a1_factor1 = (inv_beta / (dt * dt));
	double a1_factor2 = (gamma_invbeta / dt);

	double a2_factor1 = (inv_beta / dt);
	double a2_factor2 = gamma_invbeta - 1.0;

	double a3_factor1 = inv_2beta - 1.0;
	double a3_factor2 = dt * (gamma_2invbeta - 1.0);

	// Calculate the a1, a2, a3 Newmark Matrices
	a1_matrix = a1_factor1 * this->massMatrix + a1_factor2 * this->dampingMatrix;
	a2_matrix = a2_factor1 * this->massMatrix + a2_factor2 * this->dampingMatrix;
	a3_matrix = a3_factor1 * this->massMatrix + a3_factor2 * this->dampingMatrix;

	// Update the stiffness matrix
	// HHT effective stiffness
	stiffness_hat_matrix =
		((1.0 + alpha) * stiffnessMatrix) +
		(a1_factor1 * massMatrix) +
		((1.0 + alpha) * a1_factor2 * dampingMatrix);

	// Precompute Ku and Cv
	Eigen::VectorXd Ku = stiffnessMatrix * displ_at_t;
	Eigen::VectorXd Cv = dampingMatrix * velo_at_t;

	// HHT force vector
	Eigen::VectorXd force_hat_vector =
		(1.0 + alpha) * force_vector_at_t
		- alpha * force_vector_prev
		- alpha * (Ku + Cv)
		+ a1_matrix * displ_at_t
		+ a2_matrix * velo_at_t
		+ a3_matrix * accl_at_t;


	// Perform main solve
	solver.compute(stiffness_hat_matrix);

	// Solve for displacement at time t
	Eigen::VectorXd displ_i_plus1 = solver.solve(force_hat_vector);

	// Calculate the velocity 
	Eigen::VectorXd velo_i_plus1 = a1_factor2 * (displ_i_plus1 - displ_at_t)
		- a2_factor2 * velo_at_t - a3_factor2 * accl_at_t;

	// Calculate the acceleration
	Eigen::VectorXd accl_i_plus1 = a1_factor1 * (displ_i_plus1 - displ_at_t)
		- a2_factor1 * velo_at_t - a3_factor1 * accl_at_t;


	// Set the results
	this->displ_at_t = displ_i_plus1;
	this->velo_at_t = velo_i_plus1;
	this->accl_at_t = accl_i_plus1;

	// store previous force
	force_vector_prev = force_vector_at_t;

}


void hht_alpha_newmark_method::newmark_solve(double dt)
{
	// Note: The force is updated before the solve step is called

	// Find the time step factors
	double inv_beta = 1.0 / this->beta;
	double inv_2beta = 1.0 / (2.0 * this->beta);
	double gamma_invbeta = inv_beta * this->gamma;
	double gamma_2invbeta = inv_2beta * this->gamma;

	// Find the factor a1, a2, a3
	double a1_factor1 = (inv_beta / (dt * dt));
	double a1_factor2 = (gamma_invbeta / dt);

	double a2_factor1 = (inv_beta / dt);
	double a2_factor2 = gamma_invbeta - 1.0;

	double a3_factor1 = inv_2beta - 1.0;
	double a3_factor2 = dt * (gamma_2invbeta - 1.0);

	// Calculate the a1, a2, a3 Newmark Matrices
	a1_matrix = a1_factor1 * this->massMatrix + a1_factor2 * this->dampingMatrix;
	a2_matrix = a2_factor1 * this->massMatrix + a2_factor2 * this->dampingMatrix;
	a3_matrix = a3_factor1 * this->massMatrix + a3_factor2 * this->dampingMatrix;

	// Update the stiffness matrix
	// Newmark effective stiffness
	stiffness_hat_matrix = stiffnessMatrix + a1_matrix;

	// Newmark force vector
	Eigen::VectorXd force_hat_vector = force_vector_at_t
		+ a1_matrix * displ_at_t
		+ a2_matrix * velo_at_t
		+ a3_matrix * accl_at_t;


	// Perform main solve
	solver.compute(stiffness_hat_matrix);

	// Solve for displacement at time t
	Eigen::VectorXd displ_i_plus1 = solver.solve(force_hat_vector);

	// Calculate the velocity 
	Eigen::VectorXd velo_i_plus1 = a1_factor2 * (displ_i_plus1 - displ_at_t)
		- a2_factor2 * velo_at_t - a3_factor2 * accl_at_t;

	// Calculate the acceleration
	Eigen::VectorXd accl_i_plus1 = a1_factor1 * (displ_i_plus1 - displ_at_t)
		- a2_factor1 * velo_at_t - a3_factor1 * accl_at_t;


	// Set the results
	this->displ_at_t = displ_i_plus1;
	this->velo_at_t = velo_i_plus1;
	this->accl_at_t = accl_i_plus1;

}


void hht_alpha_newmark_method::update_force_vector(Eigen::VectorXd force_vector_at_timet)
{
	// Update the force at time t
	this->force_vector_at_t = force_vector_at_timet;

}





