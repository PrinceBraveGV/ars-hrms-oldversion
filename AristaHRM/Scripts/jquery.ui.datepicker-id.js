/*
 *  Inisialisasi sistem date picker regional Indonesia untuk jQuery UI
 */

// package id.arista.HRM;
// import com.jquery.*;

// class DatePickerID extends jQuery {

jQuery(function ($) {
    if (typeof $.datepicker !== 'undefined') {
        $.datepicker.regional['id'] = {
            closeText: 'Tutup',
            prevText: 'Sebelumnya',
            nextText: 'Berikutnya',
            currentText: 'Sekarang',
            monthNames: ['Januari', 'Februari', 'Maret', 'April', 'Mei', 'Juni', 'Juli', 'Agustus', 'September', 'Oktober', 'Nopember', 'Desember'],
            monthNamesShort: ['Januari', 'Februari', 'Maret', 'April', 'Mei', 'Juni', 'Juli', 'Agustus', 'September', 'Oktober', 'Nopember', 'Desember'],
            dayNames: ['Minggu', 'Senin', 'Selasa', 'Rabu', 'Kamis', 'Jumat', 'Sabtu'],
            dayNamesShort: ['Ming.', 'Sen.', 'Sel.', 'Rab.', 'Kam.', 'Jum.', 'Sab.'],
            dayNamesMin: ['M', 'SN', 'SL', 'R', 'K', 'J', 'S'],
            weekHeader: ['Pekan'],
            dateFormat: 'dd MM yy',
            firstDay: 1,
            isRTL: false,
            showMonthAfterYear: false,
            yearSuffix: ''
        };
        $.datepicker.setDefaults($.datepicker.regional['id']);
    }
});

// }