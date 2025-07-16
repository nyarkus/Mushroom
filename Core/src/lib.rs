use std::mem;
use crate::data::{Position, Size};

mod cells;
mod data;
mod grid;

#[unsafe(no_mangle)]
pub extern "C" fn set_size(width: i32, height: i32) {
    grid::grid::set_size(Size::new(width, height));
}

#[unsafe(no_mangle)]
pub extern "C" fn set_ground_level(level: i32) {
    grid::grid::set_ground_level(level);
}

#[unsafe(no_mangle)]
pub unsafe extern "C" fn get_size(width: *mut i32, height: *mut i32) {
    let size = grid::grid::get_size();
    unsafe {
        *width = size.x;
        *height = size.y;
    }
}

#[unsafe(no_mangle)]
pub unsafe extern "C" fn get_render_data(ptr: *mut *mut data::CellRenderData, len: *mut i32) {
    let mut vec = vec![];
    let size = grid::grid::get_size();
    for x in 0..=size.x {
        for y in 0..=size.y {
            let pos = Position::new(x, y);
            let color = grid::grid::get(pos).unwrap().get_color(pos);
            let data = data::CellRenderData::new(color);

            vec.push(data);
        }
    }

    unsafe {
        vec.shrink_to_fit();
        *ptr = vec.as_mut_ptr();
        *len = vec.len() as i32;

        mem::forget(vec);
    }
}

#[unsafe(no_mangle)]
pub unsafe extern "C" fn clear_render_data(ptr: *mut data::CellRenderData, len: *mut i32) {
    let len = *len as usize;
    unsafe {
        drop(Vec::from_raw_parts(ptr, len, len));
    }
}