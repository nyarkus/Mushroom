use strum_macros::EnumIter;
#[derive(Debug, Copy, Clone, PartialEq, Eq, Hash)]
pub struct Position {
    pub x: i32,
    pub y: i32,
}

impl Position {
    pub fn new(x: i32, y: i32) -> Self {
        Self { x, y }
    }
    pub fn distance_squared(&self, other: &Position) -> i32 {
        let dx = self.x - other.x;
        let dy = self.y - other.y;

        dx * dx + dy * dy
    }
}

#[derive(Debug, Copy, Clone)]
pub struct Size {
    pub x: i32,
    pub y: i32,
}

#[derive(Debug, Copy, Clone, EnumIter)]
pub enum Direction {
    Up,
    Down,
    Left,
    Right,
}