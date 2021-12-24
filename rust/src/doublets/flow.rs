use std::ops::{ControlFlow, FromResidual, Try};

pub enum Flow {
    Continue,
    Break,
}

impl FromResidual for Flow {
    fn from_residual(residual: <Self as Try>::Residual) -> Self {
        Flow::Break
    }
}

impl Try for Flow {
    type Output = ();
    type Residual = ();

    fn branch(self) -> ControlFlow<Self::Residual, Self::Output> {
        match self {
            Flow::Continue => ControlFlow::Continue(()),
            Flow::Break => ControlFlow::Break(()),
        }
    }

    fn from_output(output: Self::Output) -> Self {
        Flow::Continue
    }
}
