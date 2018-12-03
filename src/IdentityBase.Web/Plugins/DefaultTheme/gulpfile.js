var gulp = require('gulp');
var sass = require('gulp-sass');
var concat = require('gulp-concat');
var browserSync = require('browser-sync');
var uglify = require('gulp-uglify');
var sourcemaps = require('gulp-sourcemaps');

gulp.task('watch', ['build'], function () {

    return gulp.watch([
        './Public/css/**/*.scss',
        './Public/css/**/_*.scss'
    ], ['styles'])
});

gulp.task('styles', function () {

    return gulp.src('./Public/css/**/*.scss')
        .pipe(sass({ outputStyle: 'compressed' }).on('error', sass.logError))
        .pipe(gulp.dest('./Public/css/'));
});

gulp.task('scripts', function() {
    return gulp.src([
        './node_modules/bootstrap-sass/assets/javascripts/bootstrap/dropdown.js',
        './Public/js/theme.js'
    ])
        .pipe(sourcemaps.init())
        .pipe(concat('main.min.js'))
        .pipe(uglify())
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest('./Public/js/'));
});

gulp.task('lib', function () {

    // jQuery
    gulp.src('./node_modules/jquery/dist/jquery.min.*')
        .pipe(gulp.dest('./Public/js'));

    // jQuery validate
    gulp.src('./node_modules/jquery-validation/dist/jquery.validate.min.*')
        .pipe(gulp.dest('./Public/js'));

    // jQuery validate unobstrusive
    gulp.src('./node_modules/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.min.*')
        .pipe(gulp.dest('./Public/js'));

    // Bootstrap
    // No need to copy bootstrap
    // ./node_modules/bootstrap-sass
});

gulp.task('build', ['lib', 'styles', 'scripts']);

gulp.task('serve', ['build'], function () {

    browserSync.init(null, {
        proxy: "http://localhost:5000",
    });

    gulp.watch([
        './Views/**/*.cshtml',
        './Public/js/**/*.js',
        './Public/css/**/*.css'
    ]).on('change', browserSync.reload);

    gulp.watch([
        './Public/css/**/*.scss',
        './Public/css/**/_*.scss'
    ], ['styles']);
});

